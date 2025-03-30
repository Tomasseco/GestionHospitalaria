# Simulador de Atención Médica Multihilo

## Descripción

Este programa simula la atención de pacientes en un hospital utilizando programación multihilo en C#. La simulación incluye la asignación de médicos a pacientes, tiempos de consulta y el uso de máquinas de diagnóstico cuando es necesario.

El sistema gestiona:

- Un número fijo de médicos que solo pueden atender a un paciente a la vez.
- Un número limitado de máquinas de diagnóstico.
- Pacientes que llegan al hospital con tiempos de consulta predefinidos y, en algunos casos, requieren diagnóstico adicional.

## Explicación del Código

### Declaración de Variables y Recursos

```csharp
static readonly int NUM_PACIENTES = 20;
static readonly int NUM_MEDICOS = 4;
static readonly int NUM_MAQUINAS_DIAGNOSTICO = 2;
```

Se definen constantes para el número de pacientes, médicos y máquinas de diagnóstico.

```csharp
static SemaphoreSlim[] medicos = new SemaphoreSlim[NUM_MEDICOS];
static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(NUM_MAQUINAS_DIAGNOSTICO);
```

Los médicos y las máquinas de diagnóstico se gestionan con semáforos para evitar que varios pacientes utilicen el mismo recurso simultáneamente.

### Creación de Pacientes e Hilos

```csharp
Thread[] pacientes = new Thread[NUM_PACIENTES];
for (int i = 0; i < NUM_PACIENTES; i++)
```

Se crean múltiples hilos para simular la llegada y atención de los pacientes.

### Atención de Pacientes

```csharp
while (true)
{
    for (int i = 0; i < NUM_MEDICOS; i++)
    {
        if (medicos[i].Wait(0))
        {
            medicoSeleccionado = i;
            paciente.estado = Estado.Consulta;
            break;
        }
    }
    if (medicoSeleccionado != -1) break;
    Thread.Sleep(500);
}
```

Se asigna un médico disponible a un paciente, evitando bloqueos.

```csharp
if (paciente.requiereDiagnostico)
{
    lock (lockCola)
    {
        colaDiagnostico.Enqueue(regPaciente);
    }
    RealizarDiagnostico();
}
```

Si el paciente necesita diagnóstico, se agrega a una cola de espera y se inicia la evaluación.

### Diagnóstico de Pacientes

```csharp
lock (lockCola)
{
    while (colaDiagnostico.Count > 0)
    {
        RegistroPaciente regPaciente = colaDiagnostico.Peek();
        maquinasDiagnostico.Wait();
        colaDiagnostico.Dequeue();
        Thread diagnosticoThread = new Thread(() => DiagnosticoPaciente(regPaciente));
        diagnosticoThread.Start();
    }
}
```

Se gestiona la cola de diagnóstico de manera ordenada y sin bloqueos.

## Pregunta:
¿Los pacientes que deben esperar entran luego a la consulta por orden de llegada? Explica qué tipo de pruebas has realizado para comprobar este comportamiento.

### Respuesta:

Sí, los pacientes que esperan entran a la consulta por orden de llegada siempre que haya un médico disponible. Para verificarlo, se realizaron pruebas con mensajes en consola que mostraban el orden en que llegaban y eran atendidos, además de pruebas con distintos números de médicos y pacientes para comprobar que el sistema mantenía el orden esperado.

