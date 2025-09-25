## Eating Philosophers

This app is a single-thread simulation of multi-thread problem "eating philosophers"

#### Input arguments:
1. path to file of philosopher names. Count of philosophers must be greater than 1
1. Mode of work. There are 2 options "simple" (default) and "arbitrator"

#### Modes:
- In **simple** mode each philosopher just takes left and right forks. Sometimes it causes deadlocks
- In **arbitrator** all philosophers wait commands form arbitrator via C# events