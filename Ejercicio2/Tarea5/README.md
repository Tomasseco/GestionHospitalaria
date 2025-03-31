# Simulador de Atención Médica Multihilo

## Descripción

Este programa simula la atención de pacientes en un hospital utilizando programación multihilo en C#. La simulación gestiona la llegada de pacientes, la asignación de médicos y el uso de máquinas de diagnóstico cuando es necesario. Además, recopila estadísticas sobre los tiempos de espera, la atención por prioridad y el uso de máquinas de diagnóstico.

El sistema maneja:

- Un número fijo de médicos, cada uno atendiendo a un paciente a la vez.
- Un número limitado de máquinas de diagnóstico.
- Pacientes con distintas prioridades de atención.
- Estadísticas sobre el desempeño del sistema.

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

### Función `MostrarEstadisticas`

Esta función se encarga de recopilar y mostrar datos sobre la atención a los pacientes.

```csharp
static void MostrarEstadisticas()
{
    Console.WriteLine("--- FIN DEL DÍA ---");
    Console.WriteLine("Pacientes atendidos:");
    foreach (var prioridad in pacientesAtendidosPorPrioridad.Keys.OrderBy(k => k))
    {
        Console.WriteLine($"- Prioridad {prioridad}: {pacientesAtendidosPorPrioridad[prioridad]}");
    }
```

- Se imprimen los pacientes atendidos organizados por prioridad.
- Se recorren las claves del diccionario `pacientesAtendidosPorPrioridad`, ordenándolas para mostrar los resultados en orden ascendente.

```csharp
    Console.WriteLine("Tiempo promedio de espera:");
    foreach (var prioridad in tiemposEspera.Keys.OrderBy(k => k))
    {
        double promedioEspera = tiemposEspera[prioridad].Any() ? tiemposEspera[prioridad].Average() : 0;
        Console.WriteLine($"- Prioridad {prioridad}: {promedioEspera:F2}s");
    }
```

- Se calcula el tiempo promedio de espera por prioridad.
- Si no hay datos, se muestra 0.
- Se utiliza `Average()` para calcular el tiempo medio de espera de los pacientes.

```csharp
    double usoPromedioMaquinas = totalDiagnosticos > 0 ? ((double)totalUsoMaquinas / totalDiagnosticos) * 100 : 0;
    Console.WriteLine($"Uso promedio de máquinas de diagnóstico: {usoPromedioMaquinas:F2}%");
}
```

- Se calcula el uso promedio de las máquinas de diagnóstico.
- Se evita la división por cero con `totalDiagnosticos > 0`.

Esta función permite evaluar la eficiencia del sistema y detectar cuellos de botella en la atención médica simulada.

## Conclusión

El programa implementa una simulación eficiente de atención médica con múltiples pacientes, médicos y máquinas de diagnóstico. La función `MostrarEstadisticas` permite analizar el desempeño del sistema y optimizar la asignación de recursos.

