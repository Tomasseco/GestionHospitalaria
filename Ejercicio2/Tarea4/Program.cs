using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

class Program
{
    static readonly int NUM_PACIENTES = 20;
    static readonly int NUM_MEDICOS = 4;
    static readonly int NUM_MAQUINAS_DIAGNOSTICO = 2;
    
    static SemaphoreSlim[] medicos = new SemaphoreSlim[NUM_MEDICOS];
    static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(NUM_MAQUINAS_DIAGNOSTICO);
    static List<RegistroPaciente> colaEspera = new List<RegistroPaciente>();
    static object lockColaEspera = new object();
    static Random random = new Random();
    static int pacientesAtendidos = 0;
    static int totalPacientesFinalizados = 0;

    public enum Estado
    {
        EsperaConsulta,
        Consulta,
        EsperaDiagnostico,
        Diagnostico,
        Finalizado
    }

    public class Paciente
    {
        public int id { get; set; }
        public int llegadaHospital { get; set; }
        public int tiempoConsulta { get; set; }
        public bool requiereDiagnostico { get; set; }
        public Estado estado { get; set; }
        public Stopwatch cronometro { get; set; } 
        public int prioridad { get; set; }

        public Paciente(int id, int llegadaHospital, int tiempoConsulta, bool requiereDiagnostico, int prioridad)
        {
            this.id = id;
            this.llegadaHospital = llegadaHospital;
            this.tiempoConsulta = tiempoConsulta;
            this.requiereDiagnostico = requiereDiagnostico;
            this.estado = Estado.EsperaConsulta;
            this.cronometro = new Stopwatch();
            this.prioridad = prioridad;
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
        for (int i = 0; i < NUM_MEDICOS; i++)
        {
            medicos[i] = new SemaphoreSlim(1, 1);
        }

        Thread[] pacientes = new Thread[NUM_PACIENTES];
        Stopwatch cronometroGeneral = new Stopwatch();
        cronometroGeneral.Start(); 

        for (int i = 0; i < NUM_PACIENTES; i++)
        {
            int idPaciente = random.Next(10, 100); 
            int tiempoConsulta = random.Next(5, 15);
            bool requiereDiagnostico = random.Next(0, 2) == 1;
            int tiempoLlegada = (i == 0) ? 0 : cronometroGeneral.Elapsed.Seconds;
            int prioridad = random.Next(1, 4);

            Paciente paciente = new Paciente(idPaciente, tiempoLlegada, tiempoConsulta, requiereDiagnostico, prioridad);
            RegistroPaciente registroPaciente = new RegistroPaciente(i + 1, paciente);

            lock (lockColaEspera)
            {
                colaEspera.Add(registroPaciente);
                // ordeno la cola de espera por prioridad y por orden de llegada
                colaEspera = colaEspera.OrderBy(p => p.paciente.prioridad).ThenBy(p => p.ordenLlegada).ToList();
                // notifico a un hilo que esté en espera para continuar
                Monitor.Pulse(lockColaEspera);
            }
            
            pacientes[i] = new Thread(AtenderPaciente);
            pacientes[i].Start();
            Thread.Sleep(2000);
        }

        // recorremos todos los pacientes y le hacemos un join, para esperar que finalicen todos los hilos y continuar con el hilo principal.
        foreach (var pacienteThread in pacientes)
        {
            pacienteThread.Join();
        }
        
        cronometroGeneral.Stop();
        Console.WriteLine("Todos los pacientes han sido atendidos y el programa finaliza correctamente.");
    }

    static void AtenderPaciente()
    {
        while (true)
        {
            RegistroPaciente regPaciente;

            lock (lockColaEspera)
            {   // salimos si todos los pacientes han finalizados
                if (totalPacientesFinalizados >= NUM_PACIENTES)
                {
                    return;
                }
                
                while (colaEspera.Count == 0)
                {
                    // salimos si se han atendido todos los pacientes
                    if (pacientesAtendidos >= NUM_PACIENTES)
                    {
                        return;
                    }
                    // ponemos el hilo en espera
                    Monitor.Wait(lockColaEspera);
                }
                regPaciente = colaEspera[0];
                colaEspera.RemoveAt(0);
            }

            Paciente paciente = regPaciente.paciente;
            paciente.cronometro.Start();
            Console.WriteLine($"Paciente {paciente.id} (Prioridad {paciente.prioridad}). Llegado el {regPaciente.ordenLlegada}. Estado: EsperaConsulta.");

            int medicoSeleccionado = -1;
            while (true)
            {
                for (int i = 0; i < NUM_MEDICOS; i++)
                {
                    if (medicos[i].Wait(0))
                    {
                        medicoSeleccionado = i;
                        paciente.estado = Estado.Consulta;
                        Console.WriteLine($"Paciente {paciente.id} (Prioridad {paciente.prioridad}). Estado: Consulta.");
                        break;
                    }
                }
                if (medicoSeleccionado != -1) break;
                Thread.Sleep(100);
            }

            paciente.cronometro.Restart();
            Thread.Sleep(paciente.tiempoConsulta * 1000);
            paciente.estado = paciente.requiereDiagnostico ? Estado.EsperaDiagnostico : Estado.Finalizado;
            Console.WriteLine($"Paciente {paciente.id}. Estado: {(paciente.requiereDiagnostico ? "EsperaDiagnostico" : "Finalizado")}.");

            medicos[medicoSeleccionado].Release();

            lock (lockColaEspera)
            {
                pacientesAtendidos++;
                Monitor.Pulse(lockColaEspera);
            }

            if (paciente.requiereDiagnostico)
            {
                DiagnosticoPaciente(regPaciente);
            }
            else
            {
                lock (lockColaEspera)
                {
                    // incrementamos de frma segura el contador de pacientes finalizados
                    totalPacientesFinalizados++;
                }
            }
        }
    }

    static void DiagnosticoPaciente(RegistroPaciente regPaciente)
    {
        Paciente paciente = regPaciente.paciente;
        maquinasDiagnostico.Wait();
        paciente.estado = Estado.Diagnostico;
        Console.WriteLine($"Paciente {paciente.id}. Estado: Diagnostico. Esperando máquina.");
        Thread.Sleep(15000);
        paciente.estado = Estado.Finalizado;
        Console.WriteLine($"Paciente {paciente.id}. Estado: Finalizado. Diagnóstico completado.");
        maquinasDiagnostico.Release();
        
        lock (lockColaEspera)
        {
            totalPacientesFinalizados++;
            Monitor.PulseAll(lockColaEspera);
        }
    }
}
