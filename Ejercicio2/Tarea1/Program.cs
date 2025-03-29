﻿using System;
using System.Threading;
using System.Diagnostics;

class Program
{
    static readonly int NUM_PACIENTES = 4;
    static readonly int NUM_MEDICOS = 4;
    static readonly int NUM_MAQUINAS = 2;
     // Creamos los semaforos para los médicos (cada médico solo atiende a un paciente a la vez)
    static SemaphoreSlim[] medicos = new SemaphoreSlim[NUM_MEDICOS];
    static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(NUM_MAQUINAS, NUM_MAQUINAS);
    static Random random = new Random();
    
    // Enum para los estados del paciente
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

        // modifico la clase Paciente para que admita nuevo atributo rerquierDiagnistico
        public Paciente(int id, int llegadaHospital, int tiempoConsulta, bool requiereDiagnostico)
        {
            this.id = id;
            this.llegadaHospital = llegadaHospital;
            this.tiempoConsulta = tiempoConsulta;
            this.requiereDiagnostico = requiereDiagnostico;
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
           // Inicializar médicos
        for (int i = 0; i < NUM_MEDICOS; i++) {
            medicos[i] = new SemaphoreSlim(1, 1);
        }
        
        // Creamos los hilos para los pacientes
        Thread[] pacientes = new Thread[NUM_PACIENTES];
        Stopwatch cronometroGeneral = new Stopwatch();
        cronometroGeneral.Start();

        for (int i = 0; i < NUM_PACIENTES; i++) {
            int idPaciente = random.Next(10, 100);
            int tiempoConsulta = random.Next(5, 15);
           
            // aleatoriamente si necesitra diagnostico
            bool requiereDiagnostico = random.Next(0, 2) == 1;
            int tiempoLlegada = (i == 0) ? 0 : cronometroGeneral.Elapsed.Seconds;

            Paciente paciente = new Paciente(idPaciente, tiempoLlegada, tiempoConsulta, requiereDiagnostico);
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

        paciente.cronometro.Start();
        Console.WriteLine($"Paciente {paciente.id}. Llegado el {regPaciente.ordenLlegada}. Estado: EsperaConsulta. Duración: {paciente.llegadaHospital} segundos.");

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

          // Estado: Espera
        int duracionEspera = paciente.cronometro.Elapsed.Seconds;
        Console.WriteLine($"Paciente {paciente.id}. Llegado el {regPaciente.ordenLlegada}. Estado: Consulta. Duración Espera: {duracionEspera} segundos.");
        
        paciente.cronometro.Restart();
        Thread.Sleep(paciente.tiempoConsulta * 1000);
        medicos[medicoSeleccionado].Release();

        // Si requiere diagnostico
        if (paciente.requiereDiagnostico)
        {
            paciente.estado = Estado.EsperaDiagnostico;
            Console.WriteLine($"Paciente {paciente.id}. Llegado el {regPaciente.ordenLlegada}. Estado: EsperaDiagnostico.");
            
            maquinasDiagnostico.Wait();
            paciente.estado = Estado.Diagnostico;
            Console.WriteLine($"Paciente {paciente.id}. Llegado el {regPaciente.ordenLlegada}. Estado: Diagnostico. Duración: 15 segundos.");
            
            Thread.Sleep(15000);
            maquinasDiagnostico.Release();
        }

        // Estado: Finalizado
        paciente.estado = Estado.Finalizado;
        int duracionTotal = paciente.cronometro.Elapsed.Seconds;
        Console.WriteLine($"Paciente {paciente.id}. Llegado el {regPaciente.ordenLlegada}. Estado: Finalizado. Duración Total: {duracionTotal} segundos.");
    }
}
