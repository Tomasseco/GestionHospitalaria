# Simulación de Gestión Hospitalaria con Diagnóstico

## Descripción

Este programa simula la gestión de la atención hospitalaria, organizando la atención de pacientes con médicos y máquinas de diagnóstico. Los pacientes pueden requerir consultas y, en algunos casos, diagnósticos adicionales.

## Elementos del Hospital

- **Sala de espera:** Espacio donde los pacientes aguardan su turno, con una capacidad de hasta 20 personas.
- **Médicos:** 4 médicos disponibles, cada uno puede atender a un paciente a la vez.
- **Máquinas de diagnóstico:** 2 máquinas disponibles, cada una puede ser utilizada por un paciente a la vez.
- **Estados del paciente:**
  - **EsperaConsulta:** El paciente espera para ser atendido por un médico.
  - **Consulta:** El paciente está en consulta con un médico.
  - **EsperaDiagnóstico:** El paciente requiere diagnóstico y está esperando una máquina disponible.
  - **Diagnóstico:** El paciente está utilizando una máquina de diagnóstico.
  - **Finalizado:** El paciente ha terminado su consulta y/o diagnóstico.

## Explicación del Código

```csharp
using System;
using System.Threading;
using System.Diagnostics;

class Program
{
    static readonly int NUM_PACIENTES = 4;
    static readonly int NUM_MEDICOS = 4;
    static readonly int NUM_MAQUINAS = 2;
    
    static SemaphoreSlim[] medicos = new SemaphoreSlim[NUM_MEDICOS];
    static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(NUM_MAQUINAS, NUM_MAQUINAS);
    static Random random = new Random();
```
- Se definen las constantes de pacientes, médicos y máquinas de diagnóstico.
- Se crean semáforos para manejar la concurrencia en la consulta médica y las máquinas de diagnóstico.

```csharp
    public enum Estado
    {
        EsperaConsulta,
        Consulta,
        EsperaDiagnostico,
        Diagnostico,
        Finalizado
    }
```
- Se define una enumeración para representar los distintos estados por los que pasa un paciente.

```csharp
    public class Paciente
    {
        public int id { get; set; }
        public int llegadaHospital { get; set; }
        public int tiempoConsulta { get; set; }
        public bool requiereDiagnostico { get; set; }
        public Estado estado { get; set; }
        public Stopwatch cronometro { get; set; }
```
- Se define la clase `Paciente`, que almacena la información del paciente, su estado y un cronómetro para medir el tiempo de atención.

```csharp
    public class RegistroPaciente
    {
        public int ordenLlegada { get; set; }
        public Paciente paciente { get; set; }
    }
```
- La clase `RegistroPaciente` asocia un paciente con su orden de llegada.

```csharp
    static void Main()
    {
        for (int i = 0; i < NUM_MEDICOS; i++) {
            medicos[i] = new SemaphoreSlim(1, 1);
        }
```
- Se inicializan los semáforos para los médicos.

```csharp
        Thread[] pacientes = new Thread[NUM_PACIENTES];
        Stopwatch cronometroGeneral = new Stopwatch();
        cronometroGeneral.Start();
```
- Se crean los hilos para los pacientes y se inicia un cronómetro general para medir el tiempo total de ejecución.

```csharp
        for (int i = 0; i < NUM_PACIENTES; i++) {
            int idPaciente = random.Next(10, 100);
            int tiempoConsulta = random.Next(5, 15);
            bool requiereDiagnostico = random.Next(0, 2) == 1;
            int tiempoLlegada = (i == 0) ? 0 : cronometroGeneral.Elapsed.Seconds;
```
- Se generan aleatoriamente los atributos del paciente, incluyendo si requiere diagnóstico.

```csharp
            Paciente paciente = new Paciente(idPaciente, tiempoLlegada, tiempoConsulta, requiereDiagnostico);
            RegistroPaciente registroPaciente = new RegistroPaciente(i + 1, paciente);
            pacientes[i] = new Thread(AtenderPaciente);
            pacientes[i].Start(registroPaciente);
            Thread.Sleep(2000);
        }
    }
```
- Se crean hilos para cada paciente y se los inicia para su atención.

```csharp
    static void AtenderPaciente(object registro)
    {
        RegistroPaciente regPaciente = (RegistroPaciente)registro;
        Paciente paciente = regPaciente.paciente;
        paciente.cronometro.Start();
```
- Se inicia el cronómetro del paciente y se registra su llegada.

```csharp
        int medicoSeleccionado = -1;
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
- Se busca un médico disponible mediante el semáforo.

```csharp
        paciente.cronometro.Restart();
        Thread.Sleep(paciente.tiempoConsulta * 1000);
        medicos[medicoSeleccionado].Release();
```
- Se simula la consulta médica y se libera el médico al finalizar.

```csharp
        if (paciente.requiereDiagnostico)
        {
            paciente.estado = Estado.EsperaDiagnostico;
            maquinasDiagnostico.Wait();
            paciente.estado = Estado.Diagnostico;
            Thread.Sleep(15000);
            maquinasDiagnostico.Release();
        }
```
- Si el paciente necesita diagnóstico, espera hasta que haya una máquina disponible.

```csharp
        paciente.estado = Estado.Finalizado;
        Console.WriteLine($"Paciente {paciente.id} ha finalizado. Duración total: {paciente.cronometro.Elapsed.Seconds} segundos.");
    }
}
```
- Se registra el estado final del paciente y el tiempo total de su proceso.

## Conclusión

Este programa gestiona de manera concurrente la atención de pacientes, asegurando que cada uno siga su flujo correcto dentro del hospital, usando médicos y máquinas de diagnóstico de manera sincronizada mediante semáforos.


## Pregunta:
### ¿Los pacientes que deben esperar para hacerse las pruebas diagnostico entran luego a hacerse las pruebas por orden de llegada? Explica que tipo de pruebas has realizado para comprobar este comportamiento. 
- Los pacientes no entran en las máquinas de diagnóstico estrictamente por orden de llegada, sino que acceden a ellas cuando se liberan las máquinas, lo que podría ser en un orden diferente al de su llegada.

