# Gestión Hospitalaria

Este programa simula un sistema de gestión hospitalaria donde varios pacientes son atendidos por un número limitado de médicos. Cada médico puede atender a un solo paciente a la vez, y los pacientes esperan hasta que un médico esté disponible.

## Funcionamiento

El programa utiliza hilos (`Thread`) para simular pacientes que llegan al hospital y semáforos (`SemaphoreSlim`) para gestionar la disponibilidad de los médicos. Cada paciente es asignado a un médico disponible de forma aleatoria, y una vez atendido, el médico queda libre para atender a otro paciente.

### Flujo del programa

1. **Inicialización de médicos**: Se crean semáforos para cada médico, permitiendo que solo un paciente sea atendido por un médico a la vez.
    ```csharp
    for (int i = 0; i < NUM_MEDICOS; i++)
    {
         medicos[i] = new SemaphoreSlim(1, 1); 
    }
    ```

2. **Creación de pacientes**: Cada paciente es representado por un hilo que se ejecuta en paralelo. Los pacientes llegan al hospital cada 2 segundos.
    ```csharp
    for (int i = 0; i < NUM_PACIENTES; i++)
    {
         pacientes[i] = new Thread(AtenderPaciente);
         pacientes[i].Start(i + 1);
         Thread.Sleep(2000);
    }
    ```

3. **Asignación de médicos**: Cada paciente busca un médico disponible. Si no hay médicos libres, el paciente espera hasta que uno esté disponible.
    ```csharp
    while (true)
    {
         int candidato = random.Next(0, NUM_MEDICOS); 

         if (medicos[candidato].Wait(0)) 
         {
              medicoSeleccionado = candidato;
              break;
         }

         Thread.Sleep(500); 
    }
    ```

4. **Atención al paciente**: Una vez asignado un médico, el paciente es atendido durante 10 segundos. Después, el médico queda libre.
    ```csharp
    Console.WriteLine($"Paciente {paciente} entra en la consulta del Médico {medicoSeleccionado + 1}.");
    Thread.Sleep(10000);
    Console.WriteLine($"Paciente {paciente} sale de la consulta del Médico {medicoSeleccionado + 1}.");
    medicos[medicoSeleccionado].Release(); 
    ```

## Ejecución

Al ejecutar el programa, se verá cómo los pacientes llegan al hospital, esperan si no hay médicos disponibles, y son atendidos por un médico asignado. El programa utiliza mensajes en la consola para mostrar el estado de cada paciente y médico.

### Ejemplo de salida
```
Paciente 1 ha llegado.
Paciente 1 entra en la consulta del Médico 1.
Paciente 2 ha llegado.
Paciente 2 entra en la consulta del Médico 2.
Paciente 1 sale de la consulta del Médico 1.
Paciente 3 ha llegado.
Paciente 3 entra en la consulta del Médico 1.
Paciente 2 sale de la consulta del Médico 2.
Paciente 4 ha llegado.
Paciente 4 entra en la consulta del Médico 2.
```

## PREGUNTAS: 
### ¿Cuántos hilos se están ejecutando en este programa? Explica tu respuesta. 
#### - 5 hilos (1 hilo principal y 4 hilos de cada paciente)

### ¿Cuál de los pacientes entra primero en consulta? Explica tu respuesta.
#### El Paciente 1, occurre porque se crean y se inician en orden secuencial en el bucle del método Main, El hilo del paciente 1 se inicia primero y aunque los médicos se seleccionan aleatoriamente, el Paciente 1 tiene ventaja porque su hilo comienza antes que los demás. Además, hay un retraso de 2 segundos (Thread.Sleep(2000)) entre la creación de cada paciente, lo que asegura que el Paciente 1 tenga prioridad para encontrar un médico disponible.

### ¿Cuál de los pacientes sale primero de consulta? Explica tu respuesta.
#### El Paciente 1 también sale primero de consulta. Esto ocurre porque el Paciente 1 entra primero en consulta y la duración de la consulta es fija (10 segundos, definido por Thread.Sleep(10000)). Dado que no hay interrupciones ni cambios en el orden de ejecución, el Paciente 1 completa su consulta antes que los demás pacientes.
