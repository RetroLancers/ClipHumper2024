# Long Task Managers

## Overview
This directory contains various task managers for handling and distributing _long tasks_ in our application. These task managers are developed using the .NET Core framework and written in C# 12.0 language, targeting the net8.0-windows framework.

## Functionality
The primary responsibility of these task managers is to manage the quantity of _long tasks_ available and to assign them to a worker queue. Moreover, they efficiently distribute work by constantly monitoring the load on each worker and assigning new tasks to the least busy worker. This ensures an optimal balance of tasks, maximizing the throughput and performance of our application.

