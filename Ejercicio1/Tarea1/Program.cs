using System;
using System.Threading;

class Program
{
    // Constantes
    static readonly int NUM_PACIENTES = 4;
    static readonly int NUM_MEDICOS = 4;
    
    // Semaforo para los médicos 
    static SemaphoreSlim[] medicos = new SemaphoreSlim[NUM_MEDICOS];
    static Random random = new Random();

    static void Main()
    {
        // Creamos los médicos, cada medico solo puede atender 1 paciente a la vez.
        for (int i = 0; i < NUM_MEDICOS; i++)
        {
            medicos[i] = new SemaphoreSlim(1, 1); 
        }

        // Creamos los pacientes cada 2 segundos, cada paciente tiene su propio hilo para ser atendido en paralelo.
        Thread[] pacientes = new Thread[NUM_PACIENTES];

        for (int i = 0; i < NUM_PACIENTES; i++)
        {
            pacientes[i] = new Thread(AtenderPaciente);
            pacientes[i].Start(i + 1);
            Thread.Sleep(2000);
        }

        // esperamos a que todos terminen
        foreach (Thread paciente in pacientes)
        {
            paciente.Join();
        }

        Console.WriteLine("Se han atendido todos los pacientes.");
    }

    // Encontramos un médico libre
    static void AtenderPaciente(object? pacienteObj)
    {
        int paciente = Convert.ToInt32(pacienteObj);
        Console.WriteLine($"Paciente {paciente} ha llegado.");

        int medicoSeleccionado = -1;
        
        while (true)
        {
            int candidato = random.Next(0, NUM_MEDICOS); 

            if (medicos[candidato].Wait(0)) 
            {
                medicoSeleccionado = candidato;
                break;
            }

            Thread.Sleep(500); 
        }

        Console.WriteLine($"Paciente {paciente} entra en la consulta del Médico {medicoSeleccionado + 1}.");

        Thread.Sleep(10000);

        Console.WriteLine($"Paciente {paciente} sale de la consulta del Médico {medicoSeleccionado + 1}.");
        medicos[medicoSeleccionado].Release(); 
    }
}
