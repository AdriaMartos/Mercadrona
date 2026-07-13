# Mercadrona

## Overview

Mercadrona is a desktop application developed in C# using Windows Forms whose purpose is to manage and coordinate a fleet of delivery drones within a simulated environment. The application is part of the Drone Engineering Ecosystem (DEE) and uses the **csDroneLink** library to communicate with simulated drones through the MAVLink protocol.

The system has been designed to automate the entire delivery process, from customer and product management to route planning and the coordination of multiple drones operating simultaneously. To achieve this, the application allows users to create different operational scenarios, define no-fly zones, register customers and companies, generate delivery orders, and monitor the status of every drone in real time.

In addition to the automatic assignment of delivery orders, the system incorporates route-planning mechanisms capable of avoiding obstacles and restricted flight areas, as well as a conflict resolution algorithm that detects potential drone encounters and coordinates their behavior to guarantee safe operations. All this information is displayed on an interactive map, allowing the operator to monitor the complete simulation in real time.

The application has been developed following a modular design, making maintenance easier while allowing future functionalities to be incorporated with minimal changes. This architecture also enables the reuse of most components in future drone fleet management projects.

---

## Application Structure

The application is organized into a collection of forms and classes, each implementing a specific part of the system. This modular organization separates the graphical user interface from the business logic, simplifying maintenance and improving the scalability of the application.

The forms constitute the graphical user interface and are responsible for handling user interaction. Each form has been designed to perform a specific task, such as scenario creation, customer management, company administration, drone configuration, or mission monitoring.

The auxiliary classes implement the core functionality of the application. They define the data structures required to represent customers, companies, products, orders, drones, and scenarios, as well as the algorithms responsible for route planning, order assignment, and conflict resolution between drones.

Figure 7.1 of the project report illustrates the application's form hierarchy, showing the relationships between the different windows and the navigation flow followed by the user during the simulation.

---

## Application Forms

The graphical interface is composed of several forms, each designed to perform a specific function within the system.

The main form acts as the application's entry point and centralizes the management of the simulation. From this interface, users can load previously created scenarios, establish connections with the simulated drones, start and stop the simulation, and monitor the status of the fleet in real time using an interactive map.

The customer, company, and product management forms allow users to create, edit, and delete all the information required to generate delivery orders. These data are later used by the system to automatically assign deliveries to available drones.

The scenario creation form provides the necessary tools to define the simulation environment, including the flight area, drone hosts, customer locations, and restricted flight zones. The resulting configuration can be stored and reused in future simulations.

Each form has been designed to perform a single responsibility, reducing coupling between components and promoting a clear, maintainable, and extensible software architecture.

---

## Main Application Classes

In addition to the graphical interface, the application includes a set of classes responsible for implementing the business logic and managing all the information required during the simulation. These classes isolate the application's internal behavior from the user interface, making the software easier to maintain and extend.

The **Escenario** class represents the simulation environment. It stores every element belonging to a scenario, including the flight area boundaries, drone hosts, customer locations, companies, and restricted flight zones. It also provides methods for loading and saving scenario configurations in JSON format, allowing different environments to be reused without redefining them manually.

The **GestionDron** class is the core component responsible for fleet coordination. It establishes communication with the simulated drones through the csDroneLink library, continuously monitors their status, and automatically assigns delivery missions. Furthermore, it manages the complete life cycle of each drone, from takeoff to its return to the home location after completing the assigned delivery.

The **RutaDron** class implements the route-planning algorithm used by the drones during the simulation. Its main responsibility is to compute safe trajectories between two locations while avoiding restricted flight areas. To achieve this, it generates a navigation graph using the vertices of the obstacles and applies the A* search algorithm to obtain the optimal path.

The **Cliente**, **Empresa**, **Producto**, and **Pedido** classes represent the main entities involved in the delivery system. Each class stores the information required to manage the delivery process, including customer locations, available products, associated companies, and pending delivery orders.

Finally, the **Program** class serves as the application's entry point and is responsible for initializing all required resources before launching the main user interface.

---

## Implemented Features

Mercadrona integrates a complete set of functionalities that automate the entire drone delivery process within a simulated environment.

One of the application's core features is the automatic management of delivery orders. Whenever a new order is created, it is placed into a waiting queue until an available drone can execute the mission. The system continuously evaluates the status of all connected drones and assigns each order to the first drone that satisfies the necessary operational conditions.

Route planning is another fundamental component of the application. Before any mission begins, the system computes a safe trajectory between the departure point and the destination while avoiding all restricted flight zones defined within the scenario. Whenever a direct path intersects an obstacle, a navigation graph is automatically generated, and the A* algorithm is applied to compute the shortest valid route.

Throughout the simulation, the position of every drone is continuously monitored. This information allows the application to display real-time flight trajectories on the map, update mission status, and maintain a complete flight event history for each drone.

Another essential feature is the conflict detection and resolution system. The algorithm continuously monitors the distance between every active drone and detects situations in which two drones may approach each other below a predefined safety threshold. When a conflict is detected, the system determines which drone should yield while allowing the other to continue its mission, preventing collisions without requiring operator intervention.

Finally, the application includes several administration tools that enable users to create new scenarios, modify existing ones, manage customers and companies, configure products, and monitor every operation performed during the simulation.

---

## External Libraries

Several external libraries have been integrated into Mercadrona to provide the functionality required by the application.

The **csDroneLink** library is responsible for communication with the simulated drones. It provides a high-level interface for exchanging MAVLink messages and greatly simplifies drone control from the application.

The application uses **GMap.NET** to display interactive maps, draw flight paths, place markers, and visualize the real-time position of every drone during the simulation.

Configuration files are managed using **Newtonsoft.Json**, which is responsible for serializing and deserializing scenarios, customers, companies, products, and other application data.

The graphical interface has been developed using **Windows Forms** on the **.NET** platform, providing a simple and efficient desktop environment for managing the simulation.

---

## Application Deployment

Mercadrona has been developed to run on Microsoft Windows using Microsoft Visual Studio as the development environment.

Before running the project, Visual Studio must be installed with the **.NET Desktop Development** workload. Once the solution is opened, Visual Studio automatically restores all the required NuGet packages needed to compile the application.

Mission Planner and the SITL simulator must also be properly installed and configured. Mission Planner acts as the Ground Control Station and establishes communication with the simulated drones through a TCP connection. Once the simulation has started from Mission Planner, Mercadrona connects to the available drones using the csDroneLink library.

After the connection has been established, the user only needs to load one of the available scenarios, select the number of drones that will participate in the simulation, and start the system. From that point onward, Mercadrona automatically manages order assignment, route planning, and fleet coordination without requiring any further user intervention.

---

## Future Work

Although Mercadrona implements all the core functionalities required to manage a drone fleet within a simulated environment, several improvements could be incorporated in future versions.

One possible enhancement is the implementation of more advanced optimization algorithms for both order assignment and route planning, reducing delivery times as the number of drones and delivery requests increases.

Another interesting extension would be adapting the application to operate with real drones instead of a simulated environment, allowing the performance of the system to be evaluated under real operational conditions.

Future versions could also integrate database management systems to permanently store information about customers, companies, products, delivery orders, and flight histories, facilitating data analysis and long-term operation management.

Finally, additional features related to intelligent airspace management, U-Space integration, and artificial intelligence techniques could be incorporated to further improve decision-making and fleet coordination during delivery operations.
