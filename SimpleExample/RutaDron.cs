using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleExample
{
    public static class RutaDron
    {
        public class PointLatLngComparer : IEqualityComparer<PointLatLng>
        {
            public bool Equals(PointLatLng a, PointLatLng b)
            {
                return Math.Abs(a.Lat - b.Lat) < 0.000001 && Math.Abs(a.Lng - b.Lng) < 0.000001;
            }

            public int GetHashCode(PointLatLng obj)
            {
                return Math.Round(obj.Lat, 6).GetHashCode() ^ Math.Round(obj.Lng, 6).GetHashCode();
            }
        }

        public static bool PuntoEnPoligono(PointLatLng p, List<PointLatLng> poly)
        {
            bool dentro = false;
            int j = poly.Count - 1;

            for (int i = 0; i < poly.Count; i++)
            {
                if ((poly[i].Lng > p.Lng) != (poly[j].Lng > p.Lng))
                {
                    double interseccion = (poly[j].Lat - poly[i].Lat) * (p.Lng - poly[i].Lng) / (poly[j].Lng - poly[i].Lng) + poly[i].Lat;
                    if (p.Lat < interseccion)
                        dentro = !dentro;
                }
                j = i;
            }

            return dentro;
        }

        public static bool SegmentosIntersectan(PointLatLng a, PointLatLng b, PointLatLng c, PointLatLng d)
        {
            double d1 = (d.Lng - c.Lng) * (a.Lat - c.Lat) - (d.Lat - c.Lat) * (a.Lng - c.Lng);
            double d2 = (d.Lng - c.Lng) * (b.Lat - c.Lat) - (d.Lat - c.Lat) * (b.Lng - c.Lng);
            double d3 = (b.Lng - a.Lng) * (c.Lat - a.Lat) - (b.Lat - a.Lat) * (c.Lng - a.Lng);
            double d4 = (b.Lng - a.Lng) * (d.Lat - a.Lat) - (b.Lat - a.Lat) * (d.Lng - a.Lng);

            return ((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                   ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0));
        }

        public static bool CruzaPoligono(PointLatLng a, PointLatLng b, List<PointLatLng> poly)
        {
            PointLatLng puntoMedio = new PointLatLng((a.Lat + b.Lat) / 2, (a.Lng + b.Lng) / 2);
            if (PuntoEnPoligono(puntoMedio, poly))
                return true;

            for (int i = 0; i < poly.Count; i++)
            {
                var p1 = poly[i];
                var p2 = poly[(i + 1) % poly.Count];

                if (a.Equals(p1) || a.Equals(p2) || b.Equals(p1) || b.Equals(p2)) continue;

                if (SegmentosIntersectan(a, b, p1, p2))
                    return true;
            }

            return false;
        }

        public static bool SegmentoPermitido(PointLatLng a, PointLatLng b, List<List<PointLatLng>> zonasProhibidas)
        {
            foreach (var z in zonasProhibidas)
                if (CruzaPoligono(a, b, z))
                    return false;
            return true;
        }

        public static List<PointLatLng> CrearNodos(PointLatLng origen, PointLatLng destino,
                                                   List<PointLatLng> zonaPermitida,
                                                   List<List<PointLatLng>> zonasProhibidas)
        {
            var nodos = new List<PointLatLng> { origen, destino };
            nodos.AddRange(zonaPermitida);
            foreach (var z in zonasProhibidas)
                nodos.AddRange(z);

            return nodos.Distinct(new PointLatLngComparer()).ToList();
        }

        public static Dictionary<PointLatLng, List<PointLatLng>> CrearGrafo(List<PointLatLng> nodos, List<List<PointLatLng>> zonasProhibidas)
        {
            var g = new Dictionary<PointLatLng, List<PointLatLng>>(new PointLatLngComparer());
            foreach (var a in nodos)
            {
                g[a] = new List<PointLatLng>();
                foreach (var b in nodos)
                {
                    if (a.Equals(b)) continue;
                    if (SegmentoPermitido(a, b, zonasProhibidas))
                        g[a].Add(b);
                }
            }
            return g;
        }

        public static double Dist(PointLatLng a, PointLatLng b)
        {
            double dx = a.Lat - b.Lat;
            double dy = a.Lng - b.Lng;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static List<PointLatLng> AStar(PointLatLng origen, PointLatLng destino,
                                              Dictionary<PointLatLng, List<PointLatLng>> grafo)
        {
            if (!grafo.ContainsKey(origen) || !grafo.ContainsKey(destino))
                return null;

            var abiertos = new List<PointLatLng> { origen };
            var padres = new Dictionary<PointLatLng, PointLatLng>(new PointLatLngComparer());
            var g = new Dictionary<PointLatLng, double>(new PointLatLngComparer());
            var f = new Dictionary<PointLatLng, double>(new PointLatLngComparer());

            foreach (var n in grafo.Keys)
            {
                g[n] = double.MaxValue;
                f[n] = double.MaxValue;
            }
            g[origen] = 0;
            f[origen] = Dist(origen, destino);

            while (abiertos.Count > 0)
            {
                var actual = abiertos.OrderBy(x => f[x]).First();
                if (actual.Equals(destino))
                    return ReconstruirRuta(padres, destino);

                abiertos.Remove(actual);

                foreach (var vecino in grafo[actual])
                {
                    double tent = g[actual] + Dist(actual, vecino);
                    if (tent < g[vecino])
                    {
                        padres[vecino] = actual;
                        g[vecino] = tent;
                        f[vecino] = tent + Dist(vecino, destino);

                        if (!abiertos.Contains(vecino))
                            abiertos.Add(vecino);
                    }
                }
            }
            return null;
        }

        private static List<PointLatLng> ReconstruirRuta(Dictionary<PointLatLng, PointLatLng> padres, PointLatLng actual)
        {
            var ruta = new List<PointLatLng> { actual };
            while (padres.ContainsKey(actual))
            {
                actual = padres[actual];
                ruta.Add(actual);
            }
            ruta.Reverse();
            return ruta;
        }
    }
}