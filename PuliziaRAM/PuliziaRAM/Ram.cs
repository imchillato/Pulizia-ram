using System; // per le operazioni di input/output e per la gestione della console
using System.Diagnostics; // per accedere ai processi in esecuzione (come nella taskmgr)
using System.Runtime.InteropServices;  // per utilizzare la chiamata P/Invoke e interagire con le funzioni di Windows
using System.Security.Principal; // per verificare se l'utente ha i privilegi di amministratore

namespace PulisciRAM
{
    class Ram
    {
        // usiamo la chiamata P/Invoke per parlare con Windows e liberare la memoria
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        // il kernel32.dll è la libreria di windows che gestisce la memoria e il "SetProcessWorkingSetSize" è la funzione che dice a Windows "riduci la ram fin quanto puoi"
        private static extern bool SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int maximumWorkingSetSize);
        static void Main(string[] args) // il Main è la funzione principale, è qui che viene eseguito tutto il codice quando avviamo il programma
        {
            // impostiamo il titolo della console
            Console.Title = "Pulizia Ram avviato";

            // prima di tutto, controlliamo se l'utente ha i privilegi di amministratore, sennò non possiamo liberare la ram
            if (!IsAdministrator())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("esegui questo programma come amministratore prima"); // se l'utente non è amministratore, mostriamo un messaggio di errore e chiediamo di eseguire il programma con i privilegi di amministratore
                Console.ResetColor(); // resettiamo il colore della console al colore di default
                Console.ReadKey();  // aspettiamo che l'utente prema un tasto prima di chiudere la console, così ha il tempo di leggere il messaggio di errore
                return;
            }
            // se invece lo avviamo con i privilegi di amministratore, richiamiamo la funzione per pulire la ram, cioè? CleanRam();
            CleanRam();
        }

        // automaticamente CleanRam(); viene chiamato quando viene eseguit in modalità amministratore, e questa funzione è quella che si occupa di liberare la ram
        static void CleanRam()
        {
            Console.WriteLine("Analisi dei processi in corso...");
            // utilizzando la classe Process si riesce a individuare tutti i processi in esecuzione, ad esempio: Chrome, Edge, Spotify, ecc.
            Process[] processes = Process.GetProcesses();
            // una specie di "contatore" che serve a capire quanti processi sono stati ottimizzati
            int optimizedCount = 0;

            // il foreach serve per scorrere tutti i processi individuati, e ad ognuno di loro cerchiamo di liberare la ram utilizzata, se possibile
            foreach (Process proc in processes)
            {
                try
                {
                    // Passando -1 e -1, indichiamo a Windows di spostare 
                    // la memoria nel file di paging se non è strettamente necessaria
                    SetProcessWorkingSetSize(proc.Handle, -1, -1);
                    optimizedCount++; // se la chiamata è riuscita, incrementiamo il contatore dei processi ottimizzati
                }
                catch
                {
                    // ignoriamo i processi di sistema protetti, gli accessi negati inserendo un blocco try-catch, così se non possiamo ottimizzare un processo, semplicemente lo saltiamo senza far crashare il programma.
                }
            }

            // Pulizia del Garbage Collector per l'applicazione stessa, il garbage collector serve a liberare la memoria inutilizzata dall'applicazione e con queste due chiamate diciamo al garbage collector di liberare tutta la memoria che non è più necessaria.
            GC.Collect(); // forza la raccolta del garbage collector, così si libera tutta la memoria inutilizzata
            GC.WaitForPendingFinalizers(); // aspettiamo che tutte le operazioni di pulizia siano completate prima di continuare

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Completato! Ottimizzati {optimizedCount} processi."); // {optimizedCount} mostra quanti processi sono stati ottimizzati.
            Console.ResetColor();
            Console.WriteLine("Premi un tasto per uscire...");
            Console.ReadKey();
        }
        //un bool, un supporto per controllare i privilegi di amministratore, se l'utente non è amministratore, il programma mostra un messaggio di errore e si chiude.
        static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}