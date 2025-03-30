# Simulador de Atención Médica Multihilo

## Descripción

Este programa simula la atención de pacientes en un hospital mediante programación multihilo en C#. La simulación gestiona la asignación de médicos, tiempos de consulta y uso de máquinas de diagnóstico de manera concurrente, asegurando un flujo eficiente de atención.

El sistema maneja los siguientes aspectos:

- **Médicos concurrentes:** Un número fijo de médicos que solo pueden atender a un paciente a la vez.
- **Máquinas de diagnóstico limitadas:** Solo algunas pruebas requieren equipos especializados.
- **Colas de espera:** Gestión de pacientes en espera y su asignación según disponibilidad.

## Explicación del Código

### Declaración de Variables y Recursos

```csharp
static readonly int NUM_PACIENTES = 20;
static readonly int NUM_MEDICOS = 4;
static readonly int NUM_MAQUINAS_DIAGNOSTICO = 2;
```

Se establecen constantes para definir el número de pacientes, médicos y máquinas de diagnóstico que manejará el sistema.

```csharp
static SemaphoreSlim[] medicos = new SemaphoreSlim[NUM_MEDICOS];
static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(NUM_MAQUINAS_DIAGNOSTICO);
```

Se utilizan semáforos para controlar la concurrencia en el acceso a médicos y máquinas de diagnóstico, evitando que múltiples pacientes ocupen el mismo recurso simultáneamente.

### Creación de Pacientes e Hilos

```csharp
Thread[] pacientes = new Thread[NUM_PACIENTES];
for (int i = 0; i < NUM_PACIENTES; i++)
```

Se generan hilos para simular la llegada y atención de los pacientes. Cada hilo representa a un paciente que debe ser atendido por un médico.

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

Aquí se busca un médico disponible. Si un médico está libre, el paciente pasa a consulta. En caso contrario, el hilo del paciente espera y vuelve a intentarlo más tarde.

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

Si el paciente requiere diagnóstico adicional, se agrega a una cola de espera y se inicia el proceso de diagnóstico cuando una máquina esté disponible.

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

Los pacientes en espera de diagnóstico se procesan en orden. Cuando una máquina está libre, un paciente es asignado a ella, iniciando el proceso de diagnóstico en un hilo separado.

## Elección del Diseño

El diseño de esta aplicación sigue un modelo multihilo para representar de manera realista la concurrencia en un hospital:

- **Uso de Semáforos:** Controla el acceso a recursos limitados.
- **Colas de Espera:** Gestiona la asignación de pacientes de forma ordenada.
- **Sincronización de Hilos:** Evita condiciones de carrera y garantiza una atención eficiente.

Este enfoque permite optimizar los tiempos de espera y garantizar que cada paciente sea atendido sin conflictos en los recursos.

## Pregunta:

Explica el planteamiento de tu código y plantea otra posibilidad de solución a la que has programado y porqué has escogido la tuya.

### Respuesta:

El diseño utiliza semáforos para gestionar el acceso a recursos limitados como médicos y máquinas de diagnóstico, una cola de espera para gestionar pacientes que necesitan diagnóstico y bloqueos para asegurar la consistencia al manipular la cola de pacientes. Este enfoque es eficiente y evita errores de sincronización y acceso concurrente a recursos compartidos.
