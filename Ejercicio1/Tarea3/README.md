# README - Simulación de Atención Médica

## Descripción

Este programa simula un sistema de atención médica donde varios pacientes son atendidos por médicos de manera concurrente. Utiliza hilos, semáforos y cronómetros en C# para coordinar el acceso de los pacientes a los médicos disponibles y medir tiempos de espera y consulta.

## Funcionamiento

1. **Médicos:** Hay 4 médicos disponibles, cada uno puede atender solo a un paciente a la vez.
2. **Pacientes:** Se generan 4 pacientes con tiempos de llegada y duraciones de consulta aleatorias.
3. **Proceso:**
   - Cada paciente busca un médico libre y espera si todos están ocupados.
   - Se mide el tiempo de espera antes de ser atendido.
   - Una vez asignado un médico, el paciente entra en consulta por un tiempo determinado.
   - Se mide el tiempo total de consulta.
   - Cuando la consulta finaliza, el paciente se retira y el médico queda disponible para otro paciente.

## Tecnologías Utilizadas

- **Lenguaje:** C#
- **Concurrencia:** Hilos y semáforos (`Thread` y `SemaphoreSlim`)
- **Cronometraje:** `Stopwatch` para medir tiempos de espera y consulta

## Código Destacado y Explicación

### Definición de Estados y Clases de Pacientes

```csharp
public enum Estado { Espera, Consulta, Finalizado }
public class Paciente {
    public int id { get; set; }
    public int llegadaHospital { get; set; }
    public int tiempoConsulta { get; set; }
    public Estado estado { get; set; }
    public Stopwatch cronometro { get; set; }
    
    public Paciente(int id, int llegadaHospital, int tiempoConsulta) {
        this.id = id;
        this.llegadaHospital = llegadaHospital;
        this.tiempoConsulta = tiempoConsulta;
        this.estado = Estado.Espera;
        this.cronometro = new Stopwatch();
    }
}
```
**Explicación:**
- Se define una enumeración `Estado` para representar el estado de un paciente.
- La clase `Paciente` almacena información sobre el paciente, su tiempo de llegada, duración de consulta y un cronómetro para medir tiempos de espera y consulta.

### Creación de Pacientes y Asignación de Hilos

```csharp
for (int i = 0; i < NUM_PACIENTES; i++)
{
    int idPaciente = random.Next(10, 100);
    int tiempoConsulta = random.Next(5, 15);
    int tiempoLlegada = (i == 0) ? 0 : cronometroGeneral.Elapsed.Seconds;

    Paciente paciente = new Paciente(idPaciente, tiempoLlegada, tiempoConsulta);
    RegistroPaciente registroPaciente = new RegistroPaciente(i + 1, paciente);

    pacientes[i] = new Thread(AtenderPaciente);
    pacientes[i].Start(registroPaciente);
    Thread.Sleep(2000);
}
```
**Explicación:**
- Se crean hilos para simular la llegada de pacientes de forma escalonada cada 2 segundos.
- Cada paciente recibe un identificador único, un tiempo de consulta aleatorio y se registra su tiempo de llegada.
- Se inicia un hilo que llamará al método `AtenderPaciente` para gestionar la atención de cada paciente.

### Medición del Tiempo de Espera y Consulta

```csharp
// Estado: Espera
paciente.cronometro.Start();
Console.WriteLine($"Paciente {paciente.id}. Llegado el {regPaciente.ordenLlegada}. Estado: Espera. Duración: {paciente.llegadaHospital} segundos.");

// Estado: Consulta
int duracionEspera = paciente.cronometro.Elapsed.Seconds;
paciente.cronometro.Restart();
Console.WriteLine($"Paciente {paciente.id}. Llegado el {regPaciente.ordenLlegada}. Estado: Consulta. Duración Espera: {duracionEspera} segundos.");

Thread.Sleep(paciente.tiempoConsulta * 1000);

// Estado: Finalizado
int duracionConsulta = paciente.cronometro.Elapsed.Seconds;
paciente.estado = Estado.Finalizado;
Console.WriteLine($"Paciente {paciente.id}. Llegado el {regPaciente.ordenLlegada}. Estado: Finalizado. Duración Consulta: {duracionConsulta} segundos.");
medicos[medicoSeleccionado].Release();
```
**Explicación:**
- Cuando el paciente llega, se inicia un cronómetro para medir su tiempo de espera hasta que pueda ingresar a consulta.
- Una vez que el paciente encuentra un médico disponible, se mide su tiempo de espera y se reinicia el cronómetro para medir la duración de la consulta.
- Después de que transcurre el tiempo de consulta, el paciente finaliza su proceso y el médico queda libre para atender a otro paciente.

## Mejoras Posibles

- Implementar una cola de prioridad para manejar la atención según urgencia.
- Guardar el historial de consultas en un archivo de log.
- Crear una interfaz visual para visualizar la atención en tiempo real.





