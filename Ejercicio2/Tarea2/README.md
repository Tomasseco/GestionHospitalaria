# Simulador de Atención Médica Multihilo en C\#

## Descripción

Este programa simula la atención de pacientes en un hospital utilizando programación multihilo en C#. La simulación incluye la asignación de médicos a pacientes, tiempos de consulta y el uso de máquinas de diagnóstico cuando es necesario.

El sistema gestiona:

- Un número fijo de médicos que solo pueden atender a un paciente a la vez.
- Un número limitado de máquinas de diagnóstico.
- Pacientes que llegan al hospital con tiempos de consulta predefinidos y, en algunos casos, requieren diagnóstico adicional.

## Descarga y Ejecución

Para ejecutar este programa:

1. Descarga el código fuente.
2. Asegúrate de tener instalado .NET SDK.
3. Compila y ejecuta el programa con:
   ```sh
   dotnet run
   ```

## Explicación del Código

### Declaración de Variables y Recursos

```csharp
static readonly int NUM_PACIENTES = 4;
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
    int candidato = random.Next(0, NUM_MEDICOS);
    if (medicos[candidato].Wait(0))
    {
        medicoSeleccionado = candidato;
        paciente.estado = Estado.Consulta;
        break;
    }
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

## Elección del Diseño

El diseño del programa se basa en una arquitectura multihilo para representar de forma realista la concurrencia en un hospital:

- **Uso de Semáforos:** Para gestionar el acceso a recursos limitados (médicos y máquinas de diagnóstico).
- **Colas de Espera:** Para manejar pacientes que requieren diagnóstico.



**Sincronización con Bloques** **lock** : Evita condiciones de carrera en la cola de diagnóstico.

Este diseño garantiza que los pacientes sean atendidos de manera eficiente y sin bloqueos innecesarios.

## Alternativa de solución

- Otra posible solución sería utilizar un modelo basado en Programación Asíncrona con async y await
- Utilizar Task en lugar de Thread para manejar las consultas y diagnósticos de manera más eficiente.

  



