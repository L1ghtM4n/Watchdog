## :mag: Watchdog
Restart the process when it has been killed.

# :airplane: Usage
``` C#
using (var watcher = new Watcher.Watchdog())
{
    if (!watcher.IsRestarted)
        watcher.StartWatcher();


    Console.WriteLine("Press ENTER to stop watchdog");
    Console.ReadLine();

} // Watcher will stop automatically on leave this area
```


# :airplane: Example
<p align="center">
  <img src="IMG/example.gif">
</p>
