using System;
using System.Threading;
using System.Diagnostics;

class Program
{
    static readonly int NUM_PACIENTES = 4;
    static readonly int NUM_MEDICOS = 4;
    
    // Creamos los semaforos para los médicos (cada médico solo atiende a un paciente a la vez)
    static SemaphoreSlim[] medicos = new SemaphoreSlim[NUM_MEDICOS];
    static Random random = new Random();

    // Enum para los estados del paciente
    public enum Estado
    {
        Espera,
        Consulta,
        Finalizado
    }

    public class Paciente
    {
        public int id { get; set; }
        public int llegadaHospital { get; set; }
        public int tiempoConsulta { get; set; }
        public Estado estado { get; set; }
        public Stopwatch cronometro { get; set; } 

        public Paciente(int id, int llegadaHospital, int tiempoConsulta)
        {
            this.id = id;
            this.llegadaHospital = llegadaHospital;
            this.tiempoConsulta = tiempoConsulta;
            this.estado = Estado.Espera;
            this.cronometro = new Stopwatch();
        }
    }

    public class RegistroPaciente
    {
        public int ordenLlegada { get; set; }
        public Paciente paciente { get; set; }

        public RegistroPaciente(int ordenLlegada, Paciente paciente)
        {
            this.ordenLlegada = ordenLlegada;
            this.paciente = paciente;
        }
    }

    static void Main()
    {
        // Inicializar médicos
        for (int i = 0; i < NUM_MEDICOS; i++)
        {
            medicos[i] = new SemaphoreSlim(1, 1);
        }

        // Creamos los hilos para los pacientes
        Thread[] pacientes = new Thread[NUM_PACIENTES];
        Stopwatch cronometroGeneral = new Stopwatch();
        cronometroGeneral.Start(); 

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

        cronometroGeneral.Stop();
    }

    static void AtenderPaciente(object registro)
    {
        RegistroPaciente regPaciente = (RegistroPaciente)registro;
        Paciente paciente = regPaciente.paciente;

        // Estado: Espera
        paciente.cronometro.Start();
        Console.WriteLine($"Paciente {paciente.id}. Llegado el {regPaciente.ordenLlegada}. Estado: Espera. Duración: {paciente.llegadaHospital} segundos.");

        int medicoSeleccionado = -1;

        // Buscar un médico libre
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

        // Estado: Consulta
        int duracionEspera = paciente.cronometro.Elapsed.Seconds;
        Console.WriteLine($"Paciente {paciente.id}. Llegado el {regPaciente.ordenLlegada}. Estado: Consulta. Duración Espera: {duracionEspera} segundos.");
        
        paciente.cronometro.Restart();
        Thread.Sleep(paciente.tiempoConsulta * 1000);

        // Estado: Finalizado
        int duracionConsulta = paciente.cronometro.Elapsed.Seconds;
        paciente.estado = Estado.Finalizado;
        Console.WriteLine($"Paciente {paciente.id}. Llegado el {regPaciente.ordenLlegada}. Estado: Finalizado. Duración Consulta: {duracionConsulta} segundos.");

        medicos[medicoSeleccionado].Release();
    }
}
