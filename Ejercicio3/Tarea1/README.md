# Simulador de Atención Médica Multihilo en C#

## Descripción

Este programa simula la atención de pacientes en un hospital utilizando concurrencia en C#. Se gestiona la llegada de pacientes, la asignación de médicos y el uso de máquinas de diagnóstico. Además, se registran estadísticas sobre tiempos de espera, atención por prioridad y uso de máquinas de diagnóstico.

El sistema maneja:

- Un generador de pacientes que genera uno nuevo cada 2 segundos.
- Médicos concurrentes, cada uno atendiendo a un paciente a la vez.
- Máquinas de diagnóstico limitadas.
- Pacientes con distintas prioridades de atención.
- Recopilación de estadísticas del proceso.

## Ejecución

1. Descarga el código fuente.
2. Asegúrate de tener instalado .NET SDK.
3. Compila y ejecuta el programa con:
   ```sh
   dotnet run
   ```

## Explicación del Código

### 1. Generador de Pacientes

El generador de pacientes crea un nuevo paciente cada 2 segundos con valores aleatorios para:
- Tiempo de consulta.
- Prioridad (1, 2 o 3).
- Si requiere o no diagnóstico con máquina.

```csharp
Thread.Sleep(2000);
Paciente paciente = new Paciente(idPaciente, tiempoLlegada, tiempoConsulta, requiereDiagnostico, prioridad);
```

### 2. Uso de Semáforos para Controlar Recursos

Los semáforos se usan para controlar el acceso concurrente de médicos y máquinas de diagnóstico:

```csharp
static SemaphoreSlim[] medicos = new SemaphoreSlim[NUM_MEDICOS];
static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(NUM_MAQUINAS_DIAGNOSTICO);
```

Esto evita que dos pacientes utilicen el mismo recurso al mismo tiempo.

### 3. Atención de Pacientes

Cada paciente es atendido por un médico y, si es necesario, pasa a diagnóstico:

```csharp
paciente.estado = Estado.Consulta;
Thread.Sleep(paciente.tiempoConsulta * 1000);
if (paciente.requiereDiagnostico) { maquinasDiagnostico.Wait(); }
```

Los pacientes con mayor prioridad son atendidos primero.

### 4. Registro de Estadísticas

Se recopilan datos sobre:
- Tiempo de espera por prioridad.
- Uso de máquinas de diagnóstico.

```csharp
double promedioEspera = tiemposEspera[prioridad].Any() ? tiemposEspera[prioridad].Average() : 0;
```

Esto permite evaluar la eficiencia del sistema.

## Evaluación del Programa

### Tarea 1: ¿Cumple Requisitos?

**Pruebas:** Se ejecutó con N = 50, 100, 1000 pacientes.

**Explicación:** Se confirma que el generador de pacientes y la concurrencia funcionan correctamente, asegurando que los pacientes sean atendidos en el orden correcto.

### Tarea 2: ¿Qué comportamientos no previstos detectas?

**Pruebas:** En pruebas con N = 1000, se observó saturación de médicos y acumulación de pacientes en espera.
También se observa que la ID de cada paciente se repite en prueba de 100 y 1000 pacientes.

**Explicación:** A medida que aumentan los pacientes, los tiempos de espera crecen debido a la disponibilidad limitada de recursos.

### Tarea 3: ¿Cómo adaptarías tu solución?

**Explicación:** Se podría implementar una lógica dinámica para asignar médicos adicionales según la demanda, mejorando la eficiencia en escenarios con muchos pacientes.
Para la ID de paciente se podría implementar una función que a razón de los pacientes pedidos proporcione un ID único en ese rango, o que genere matriculas como aparece en las salas de espera de los hospitales, ejm: TSC01.

