# README - Simulación de Atención Médica

## Descripción

Este programa simula un sistema de atención médica donde varios pacientes son atendidos por médicos de manera concurrente. Utiliza hilos y semáforos en C# para coordinar el acceso de los pacientes a los médicos disponibles.

## Funcionamiento

1. **Médicos:** Hay 4 médicos disponibles, cada uno puede atender solo a un paciente a la vez.
2. **Pacientes:** Se generan 4 pacientes con tiempos de llegada y duraciones de consulta aleatorias.
3. **Proceso:**
   - Cada paciente busca un médico libre y espera si todos están ocupados.
   - Una vez asignado un médico, el paciente entra en consulta por un tiempo determinado.
   - Cuando la consulta finaliza, el paciente se retira y el médico queda disponible para otro paciente.

## Tecnologías Utilizadas

- **Lenguaje:** C#
- **Concurrencia:** Hilos y semáforos (`Thread` y `SemaphoreSlim`)
- **Cronometraje:** `Stopwatch` para medir tiempos de llegada

## Instalación y Uso

1. Clona este repositorio o descarga el código fuente:
   ```sh
   git clone https://github.com/tu-usuario/simulacion-hospital.git
   ```
2. Abre el proyecto en **Visual Studio** o cualquier entorno compatible con C#.
3. Compila y ejecuta el programa.

## Código Destacado

### Definición de Estados y Clases de Pacientes

```csharp
public enum Estado { Espera, Consulta, Finalizado }
public class Paciente {
    public int id { get; set; }
    public int llegadaHospital { get; set; }
    public int tiempoConsulta { get; set; }
    public Estado estado { get; set; }
    public Paciente(int id, int llegadaHospital, int tiempoConsulta) {
        this.id = id;
        this.llegadaHospital = llegadaHospital;
        this.tiempoConsulta = tiempoConsulta;
        this.estado = Estado.Espera;
    }
}
```

### Creación de Pacientes y Asignación de Hilos

```csharp
for (int i = 0; i < NUM_PACIENTES; i++)
{
    int idCliente = random.Next(0, 100);
    int tiempoConsulta = random.Next(5, 15);
    int tiempoLlegada = (i == 0) ? 0 : cronometro.Elapsed.Seconds;

    Paciente paciente = new Paciente(idCliente, tiempoLlegada, tiempoConsulta);
    RegistroPaciente nuevoRegistroPaciente = new RegistroPaciente(i, paciente);
    pacientes[i] = new Thread(AtenderPaciente);
    pacientes[i].Start(nuevoRegistroPaciente);
    Thread.Sleep(2000);
}
```

### Simulación de la Consulta y Liberación del Médico

```csharp
Thread.Sleep(tiempoConsulta * 1000);
regPaciente.paciente.estado = Estado.Finalizado;

Console.WriteLine($"Paciente ID:{idPaciente} sale de la consulta del Médico {medicoSeleccionado + 1} | " +
     $"Tiempo de consulta: {tiempoConsulta} seg. | Estado {regPaciente.paciente.estado}\n");
medicos[medicoSeleccionado].Release();
```

## Mejoras Posibles

- Implementar una cola de prioridad para manejar la atención según urgencia.
- Guardar el historial de consultas en un archivo de log.
- Crear una interfaz visual para visualizar la atención en tiempo real.

## PREGUNTA:

### ¿Cuál de los pacientes sale primero de consulta? Explica tu respuesta.
- Dado que el código asigna un tiempo de consulta aleatorio entre 5 y 15 segundos, el primer paciente que logre obtener un médico y tenga el menor tiempo de consulta terminará antes.

