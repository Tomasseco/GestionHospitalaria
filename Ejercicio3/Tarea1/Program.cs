using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

class Program
{
    static readonly int NUM_PACIENTES = 50; // Cambiar para las pruebas 50 / 100 / 1000
    static readonly int NUM_MEDICOS = 4;
    static readonly int NUM_MAQUINAS_DIAGNOSTICO = 2;
    
    static SemaphoreSlim[] medicos = new SemaphoreSlim[NUM_MEDICOS];
    static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(NUM_MAQUINAS_DIAGNOSTICO);
    static List<RegistroPaciente> colaEspera = new List<RegistroPaciente>();
    static object lockColaEspera = new object();
    static Random random = new Random();
    // Los diccionarios para las estadisticas, es lo más parecido que he encontrado al HashMap de Java.
    static Dictionary<int, int> pacientesAtendidosPorPrioridad = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 } };
    static Dictionary<int, List<int>> tiemposEspera = new Dictionary<int, List<int>> { { 1, new List<int>() }, { 2, new List<int>() }, { 3, new List<int>() } };
    static int totalUsoMaquinas = 0;
    static int totalDiagnosticos = 0;
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
        public int prioridad { get; set; }
        public Estado estado { get; set; }
        public Stopwatch cronometro { get; set; }

        public Paciente(int id, int llegadaHospital, int tiempoConsulta, bool requiereDiagnostico, int prioridad)
        {
            this.id = id;
            this.llegadaHospital = llegadaHospital;
            this.tiempoConsulta = tiempoConsulta;
            this.requiereDiagnostico = requiereDiagnostico;
            this.prioridad = prioridad;
            this.estado = Estado.EsperaConsulta;
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
                colaEspera = colaEspera.OrderBy(p => p.paciente.prioridad).ThenBy(p => p.ordenLlegada).ToList();
                Monitor.PulseAll(lockColaEspera);
            }

            pacientes[i] = new Thread(AtenderPaciente);
            pacientes[i].Start();
            Thread.Sleep(2000);
        }

        foreach (var pacienteThread in pacientes)
        {
            pacienteThread.Join();
        }

        cronometroGeneral.Stop();
        MostrarEstadisticas();
        Console.WriteLine("Todos los pacientes han sido atendidos y el programa finaliza correctamente.");
    }

    static void MostrarEstadisticas()
    {
        Console.WriteLine("--- FIN DEL DÍA ---");
        Console.WriteLine("Pacientes atendidos:");
        foreach (var prioridad in pacientesAtendidosPorPrioridad.Keys.OrderBy(k => k))
        {
            Console.WriteLine($"- Prioridad {prioridad}: {pacientesAtendidosPorPrioridad[prioridad]}");
        }

        Console.WriteLine("Tiempo promedio de espera:");
        foreach (var prioridad in tiemposEspera.Keys.OrderBy(k => k))
        {
            double promedioEspera = tiemposEspera[prioridad].Any() ? tiemposEspera[prioridad].Average() : 0;
            Console.WriteLine($"- Prioridad {prioridad}: {promedioEspera:F2}s");
        }

        double usoPromedioMaquinas = totalDiagnosticos > 0 ? ((double)totalUsoMaquinas / totalDiagnosticos) * 100 : 0;
        Console.WriteLine($"Uso promedio de máquinas de diagnóstico: {usoPromedioMaquinas:F2}%");
    }

    static void AtenderPaciente()
    {
        while (true)
        {
            RegistroPaciente regPaciente;

            lock (lockColaEspera)
            {
                if (totalPacientesFinalizados >= NUM_PACIENTES)
                {
                    return;
                }

                if (colaEspera.Count == 0)
                {
                    return;
                }

                regPaciente = colaEspera[0];
                colaEspera.RemoveAt(0);
            }

            Paciente paciente = regPaciente.paciente;
            paciente.cronometro.Start();
            Console.WriteLine($"Paciente {paciente.id} (Prioridad {paciente.prioridad}). Llegado el {regPaciente.ordenLlegada}. Estado: EsperaConsulta.");

            Thread.Sleep(paciente.tiempoConsulta * 1000);
            paciente.estado = Estado.Consulta;
            Console.WriteLine($"Paciente {paciente.id}. Estado: Consulta.");

            lock (lockColaEspera)
            {
                // Actualizamos las estadisicas por prioridad
                pacientesAtendidosPorPrioridad[paciente.prioridad]++;
                tiemposEspera[paciente.prioridad].Add(paciente.cronometro.Elapsed.Seconds);
            }

            if (paciente.requiereDiagnostico)
            {
                maquinasDiagnostico.Wait();
                // Actualizamos el uso de máquinas para calcular las estadisticas.
                totalUsoMaquinas++;
                Thread.Sleep(random.Next(3, 7) * 1000);
                maquinasDiagnostico.Release();
                totalDiagnosticos++;
                paciente.estado = Estado.Diagnostico;
                Console.WriteLine($"Paciente {paciente.id}. Estado: Diagnostico.");
            }

            paciente.estado = Estado.Finalizado;
            Console.WriteLine($"Paciente {paciente.id}. Estado: Finalizado.");
            totalPacientesFinalizados++;
        }
    }
}
