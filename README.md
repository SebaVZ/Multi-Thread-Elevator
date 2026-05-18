# 🏢 Elevator Control System

A multithreaded elevator management system built in **C# / WinForms** that simulates the concurrent operation of multiple elevators across multiple buildings — with priority queuing, special override commands, and real-time visual feedback.

---

## 📌 Overview

This system models a real-world elevator control scenario where several elevators operate simultaneously across different buildings. Each elevator runs on its own thread, processes requests independently, and must coordinate with other elevators when special conditions apply.

The application was designed to explore and apply core concepts of **concurrent programming**: thread synchronization, mutual exclusion, priority scheduling, and inter-thread communication.

---

## ✨ Features

### Core Behavior
- **Multi-building support** — configure any number of buildings, each with its own set of elevators
- **Concurrent execution** — every elevator runs on a dedicated thread, operating in parallel with all others
- **Smart request scheduling** — if a stop can be served during an active route without detour, it is picked up on the way

### Request Types
| Type | Description |
|---|---|
| Standard | Floor-to-floor transfer request, queued and processed in order |
| In-transit | External requests submitted while an elevator is moving, dynamically inserted into the route |
| Priority Override | Special command that bypasses lower-priority requests; requires a specific key sequence to activate |
| Emergency Stop | Halts all or individual elevators instantly, regardless of current state |

### Priority System
- Requests are evaluated and ordered by priority at runtime
- A **priority override** command causes the elevator to skip pending lower-priority stops
- While one elevator in a building executes a priority override, **no other elevator in that building can do the same** (mutual exclusion)

### Pause & Resume
- The entire system (or individual elevators) can be paused at any moment
- New requests can still be queued while paused
- On resume, each elevator continues exactly from where it stopped

### Control Panels
- Every **floor** has an external panel to request pickup
- Every **elevator cabin** has an internal panel to select destination floors, open/close doors, go to rooftop or ground floor

### Visual Interface (WinForms)
- Real-time graphical display of each elevator's position within its building
- Live data panel per elevator: current floor, request queue, next stop, and request priority
- Adjustable thread execution speed for observation and testing
- Configurable number of buildings and elevators at startup

---

## 🛠️ Tech Stack

| | |
|---|---|
| **Language** | C# (.NET) |
| **UI Framework** | Windows Forms (WinForms) |
| **Concurrency** | System.Threading — `Thread`, `Monitor`, `lock`, `Mutex` |
| **Graphics** | System.Drawing — GDI+ for real-time elevator rendering |

---

## 🚀 Getting Started

### Prerequisites
- Windows OS
- .NET Framework (4.7.2 or higher recommended)
- Visual Studio Community 2022 or later

### Run
```bash
# Clone the repository
git clone https://github.com/SebaVZ/Multi-Thread-Elevator.git

# Open the solution in Visual Studio
# Build and run with F5
```

At startup, enter the number of **buildings** and **elevators per building**. The simulation begins immediately.

---

## 🧠 Concepts Applied

- **Multithreading** — independent thread per elevator for true parallel execution
- **Thread synchronization** — `lock` and `Monitor` to prevent race conditions on shared queues
- **Mutual exclusion** — `Mutex` to enforce the single-priority-override constraint per building
- **Priority scheduling** — dynamic reordering of request queues at runtime
- **Producer-consumer pattern** — UI and panels produce requests; elevator threads consume them

---

## 📁 Project Structure

```
Multi-Thread-Elevator/
├── Models/
│   ├── Edificio.cs          # Building container, manages elevator collection
│   ├── Solicitud.cs         # Request to move elevator
├── Core/
│   ├── Elevator.cs          # Elevator thread logic and state machine
│   ├── Solicitud.cs         # Request to move elevator
├── Core/
│   ├── PanelDeControl.cs            # Example Control Panel for elevator
│   ├── PanelDeControlUniversal.cs   # Currently used control panel for all elevators
├── FormAscensores.cs        # Principal screen with all buildings and elevator
├── FormConfiguracion.cs     # First screen to set the amount of buildings, elevators and floors.
├── Program.cs
```

---

## 📚 Academic Context

Developed as a university project for a **operating systems** course at Universidad Nacional de Costa Rica (UNA). The goal was to apply threading concepts to a real-world simulation with non-trivial synchronization requirements.

---

## 👤 Author

**Sebastián Vega Zúñiga**
[github.com/SebaVZ](https://github.com/SebaVZ) · [linkedin.com/in/sebastián-vega-927896351](https://linkedin.com/in/sebastián-vega-927896351)
