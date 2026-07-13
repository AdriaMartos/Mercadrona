using GMap.NET;
using System.Collections.Generic;

namespace SimpleExample
{

	public class Host
	{
		public string Nombre { get; set; }
		public double Lat { get; set; }
		public double Lng { get; set; }

		public PointLatLng Posicion => new PointLatLng(Lat, Lng);
	}

    public class Cliente
    {
        public string Nombre { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Direccion { get; set; }
        public PointLatLng Posicion => new PointLatLng(Latitud, Longitud);

        public string ObtenerHostCercano(List<Host> hosts)
        {
            if (hosts == null || hosts.Count == 0) return "Sin Host";

            Host masCercano = null;
            double distanciaMinima = double.MaxValue;

            foreach (var h in hosts)
            {

                double dLat = h.Lat - this.Latitud;
                double dLng = h.Lng - this.Longitud;
                double distancia = (dLat * dLat) + (dLng * dLng);

                if (distancia < distanciaMinima)
                {
                    distanciaMinima = distancia;
                    masCercano = h;
                }
            }

            return masCercano != null ? masCercano.Nombre : "Sin Host";
        }
    }

    public class Escenario
	{
		public string Nombre { get; set; }

		public int ZoomInicial { get; set; } = 20;   

		public PointLatLng BaseDron { get; set; }

		public List<PointLatLng> ZonaPermitida { get; set; }
		public List<List<PointLatLng>> ZonasProhibidas { get; set; }

		public List<Host> Hosts { get; set; }
		public List<Cliente> Clientes { get; set; }

		public Escenario()
		{
			ZonaPermitida = new List<PointLatLng>();
			ZonasProhibidas = new List<List<PointLatLng>>();
			Hosts = new List<Host>();
			Clientes = new List<Cliente>();
		}
	}


	public static class EscenariosFactory
	{
		public static Escenario CrearDronLab()
		{
			return new Escenario
			{
				Nombre = "DroneLab",
				ZoomInicial = 20,
				BaseDron = new PointLatLng(41.276407, 1.988615),

				ZonaPermitida = new List<PointLatLng>
				{
					new PointLatLng(41.27642274139705, 1.98857284582421),
					new PointLatLng(41.27637636763656, 1.98859307709327),
					new PointLatLng(41.27639126853962, 1.988657797682578),
					new PointLatLng(41.27643804028069, 1.988638025864495)
				},

				Hosts = new List<Host>
				{
					new Host { Nombre = "HOST 1", Lat = 41.27640009011409, Lng = 1.988328109036535 },
					new Host { Nombre = "HOST 2", Lat = 41.27623128567483, Lng = 1.988414481229792 },
					new Host { Nombre = "HOST 3", Lat = 41.27653529948392, Lng = 1.988917742145713 },
					new Host { Nombre = "HOST 4", Lat = 41.27638188914773, Lng = 1.988949760131036 }
				},

				ZonasProhibidas = new List<List<PointLatLng>>
				{
					new List<PointLatLng>
					{
						new PointLatLng(41.27630536572202, 1.988437036335955),
						new PointLatLng(41.27630096271997, 1.988418125929494),
						new PointLatLng(41.27626851962627, 1.988434801618923),
						new PointLatLng(41.27627229611673, 1.988449000920278),
						new PointLatLng(41.27630536572202, 1.988437036335955)
					},

					new List<PointLatLng>
					{
						new PointLatLng(41.27631071455258, 1.988507262495423),
						new PointLatLng(41.27630172993552, 1.988474610657389),
						new PointLatLng(41.27626837039866, 1.98849430643302),
						new PointLatLng(41.27627920548493, 1.988529656809106),
						new PointLatLng(41.27631071455258, 1.988507262495423)
					},

					new List<PointLatLng>
					{
						new PointLatLng(41.27643141605329, 1.988384087801598),
						new PointLatLng(41.27642364276444, 1.988360824434825),
						new PointLatLng(41.27636284903624, 1.988395472071824),
						new PointLatLng(41.27635143627969, 1.988411277777777),
						new PointLatLng(41.27643141605329, 1.988384087801598)
					},

					new List<PointLatLng>
					{
						new PointLatLng(41.27652312450065, 1.988810959903304),
						new PointLatLng(41.2764556827353, 1.988802272305843),
						new PointLatLng(41.27647661522857, 1.988959539817157),
						new PointLatLng(41.27652312450065, 1.988810959903304)
					},

					new List<PointLatLng>
					{
						new PointLatLng(41.27641880864586, 1.988756042565687),
						new PointLatLng(41.27636674832786, 1.98872108598166),
						new PointLatLng(41.27636877781778, 1.98875296426851),
						new PointLatLng(41.27641880864586, 1.988756042565687)
					},

					new List<PointLatLng>
					{
						new PointLatLng(41.27643753407584, 1.988842213376913),
						new PointLatLng(41.27639281341571, 1.98881374427222),
						new PointLatLng(41.2763526535381, 1.988835992948081),
						new PointLatLng(41.27643753407584, 1.988842213376913)
					}
				},


				Clientes = new List<Cliente>
				{
					 new Cliente { Nombre = "CARLOS",  Latitud = 41.27645500508403, Longitud = 1.988366367261563 },
					 new Cliente { Nombre = "CLAUDIA", Latitud = 41.27636369527087, Longitud = 1.988269776195117 },
					 new Cliente { Nombre = "ALEIX",   Latitud = 41.27628082196664, Longitud = 1.988337736168286 },
					 new Cliente { Nombre = "ALBERT",  Latitud = 41.27621013320926, Longitud = 1.988410369641942 },
					 new Cliente { Nombre = "AINARA",  Latitud = 41.27633051908089, Longitud = 1.988912468625252 },
					 new Cliente { Nombre = "NIL",     Latitud = 41.27643242667695, Longitud = 1.989024327766622 },
					 new Cliente { Nombre = "KATIA",   Latitud = 41.27653656104773, Longitud = 1.988776580121543 },
					 new Cliente { Nombre = "LARA",    Latitud = 41.27656590775075, Longitud = 1.988983719305988 }
				}
					
			};
		}


		public static Escenario CrearMercadrona()
		{
			return new Escenario
			{
				Nombre = "Mercadrona",
				ZoomInicial = 15,
                BaseDron = new PointLatLng(41.2820, 1.9856),

                ZonaPermitida = new List<PointLatLng>
				{
					new PointLatLng(41.28232485045768, 1.985297307072376),
					new PointLatLng(41.28170014126412, 1.985424581340061),
					new PointLatLng(41.28190712439082, 1.985990502898156),
					new PointLatLng(41.28236448851187, 1.985863820469769)
				},

				Hosts = new List<Host>
				{
					new Host { Nombre = "Host A", Lat = 41.27920743280772, Lng = 1.9691957656 },
					new Host { Nombre = "Host B", Lat = 41.27200393007661, Lng = 1.973862117804102 },
					new Host { Nombre = "Host C", Lat = 41.28880839395411, Lng = 1.982359179208331 }
				},
                ZonasProhibidas = new List<List<PointLatLng>>
				{

					new List<PointLatLng>
					{
						new PointLatLng(41.28089030965151, 1.973065061577521),
						new PointLatLng(41.27949513708577, 1.971617382135278),
						new PointLatLng(41.27924123665218, 1.97517123013536),
						new PointLatLng(41.2822915016828, 1.97600026097194)
					},

					new List<PointLatLng>
					{
						new PointLatLng(41.28592990859485, 1.982132733481721),
						new PointLatLng(41.28470335407916, 1.982559865872697),
						new PointLatLng(41.2853914450633, 1.990160940848855),
						new PointLatLng(41.2865624601586, 1.989918173133665)
					},

					new List<PointLatLng>
					{
						new PointLatLng(41.28763089097436, 1.978155407382511),
						new PointLatLng(41.28633946452028, 1.978052669207284),
						new PointLatLng(41.28702602705409, 1.985066358277547),
						new PointLatLng(41.28802856846436, 1.984886548633773)
					},

					new List<PointLatLng>
					{
						new PointLatLng(41.275773013079, 1.973762227961444),
						new PointLatLng(41.27351234989039, 1.979118063803849),
						new PointLatLng(41.27669387092138, 1.97753352253961)
					},

					new List<PointLatLng>
					{
						new PointLatLng(41.2782264353691, 1.977110450781263),
						new PointLatLng(41.27624012313641, 1.987675857803191),
						new PointLatLng(41.27895646679915, 1.982488521866235)
					}
				},
                Clientes = new List<Cliente>
				{
					new Cliente { Nombre = "Josep",  Latitud = 41.28410039408757, Longitud = 1.969001816385312 },
					new Cliente { Nombre = "Jhon",   Latitud = 41.2775219604674,  Longitud = 1.965326438747477 },
					new Cliente { Nombre = "Laia",   Latitud = 41.28967074205236, Longitud = 1.985162643447265 },
					new Cliente { Nombre = "Lidia",  Latitud = 41.28931574482213, Longitud = 1.977030629651977 },
					new Cliente { Nombre = "Marc",   Latitud = 41.26919773132028, Longitud = 1.970266697699561 },
					new Cliente { Nombre = "Martos", Latitud = 41.27034550014028, Longitud = 1.978123311176778 }
				}
			};
		}
	}
}
