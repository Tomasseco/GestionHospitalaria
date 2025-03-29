using System;
using System.Threading;
using System.Diagnostics;

class Program
{
    // Constantes
    static readonly int NUM_PACIENTES = 4;
    static readonly int NUM_MEDICOS = 4;
    
    // Semaforo para los médicos 
    static SemaphoreSlim[] medicos = new SemaphoreSlim[NUM_MEDICOS];
    static Random random = new Random();

    public enum Estado
    {
        Espera,
        Consulta,
        Finalizado
    }
    public class Paciente {
        public int id { get; set; }
        public int llegadaHospital { get; set; }
        public int tiempoConsulta { get; set; }
        public Estado estado{get; set; }

        public Paciente(int id, int llegadaHospital, int tiempoConsulta) {
            this.id = id;
            this.llegadaHospital = llegadaHospital;
            this.tiempoConsulta = tiempoConsulta;
            this.estado = Estado.Espera;
        }
        
    }

    public class RegistroPaciente {

        public int ordenLlegada { get; set; }
        public Paciente paciente{ get; set; }
        public RegistroPaciente(int ordenLlegada, Paciente paciente) {
            this.ordenLlegada = ordenLlegada;
            this.paciente = paciente; 
        }
    }
  
    static void Main()
    {
        // Creamos los médicos, cada medico solo puede atender 1 paciente a la vez.
        for (int i = 0; i < NUM_MEDICOS; i++)
        {
            medicos[i] = new SemaphoreSlim(1, 1); 
        }

        // Creamos los pacientes cada 2 segundos, cada paciente tiene su propio hilo para ser atendido en paralelo.
        Thread[] pacientes = new Thread[NUM_PACIENTES];
        
        // Creamos un cronometro para llevar el control del tiempo de las llegadas de los pacientes
        Stopwatch cronometro = new Stopwatch();
        cronometro.Start(); 
        
        for (int i = 0; i < NUM_PACIENTES; i++)
        {
            int idCliente = random.Next(0, 100);
            int tiempoConsulta = random.Next(5, 15);        
            int tiempoLlegada = (i==0)?0:cronometro.Elapsed.Seconds;
    
            Paciente paciente = new Paciente(idCliente, tiempoLlegada, tiempoConsulta);

            RegistroPaciente nuevoRegistroPaciente = new RegistroPaciente(i, paciente);
            pacientes[i] = new Thread(AtenderPaciente);
            pacientes[i].Start(nuevoRegistroPaciente);

            Thread.Sleep(2000);
        }
        cronometro.Stop();
    }

    // Encontramos un médico libre
    static void AtenderPaciente(object registroPaciente)
    {
        RegistroPaciente regPaciente = (RegistroPaciente) registroPaciente;  
        int idPaciente = regPaciente.paciente.id;
        int tiempoConsulta = regPaciente.paciente.tiempoConsulta;
        int ordenLlegada = regPaciente.ordenLlegada;
        
        Console.WriteLine($"Paciente ID:{idPaciente} ha llegado con el orden: {ordenLlegada} | Estado {regPaciente.paciente.estado}\n");

        int medicoSeleccionado = -1;
        
        while (true)
        {
            int candidato = random.Next(0, NUM_MEDICOS); 

            if (medicos[candidato].Wait(0)) 
            {
                medicoSeleccionado = candidato;
                regPaciente.paciente.estado = Estado.Consulta;
                break;
            }

        }

        Console.WriteLine($"Paciente ID:{idPaciente} entra en la consulta del Médico {medicoSeleccionado + 1} | Estado {regPaciente.paciente.estado}\n");

        Thread.Sleep(tiempoConsulta * 1000);
        regPaciente.paciente.estado = Estado.Finalizado;

        Console.WriteLine($"Paciente ID:{idPaciente} sale de la consulta del Médico {medicoSeleccionado + 1} | "+
         $"Tiempo de consulta: {tiempoConsulta} seg. | Estado {regPaciente.paciente.estado}\n");
        medicos[medicoSeleccionado].Release(); 
    }
}
