using ELWS.Core.Abstractions;
using PavonisInteractive.TerraInvicta.Debugging;

namespace ELWS.Debugging;

public class TerminalElwsCommands : IModClass
{
    // ReSharper disable once NotAccessedField.Local
    private readonly TerminalController _terminalController;
    // private bool _isActive = true;

    public TerminalElwsCommands(TerminalController terminalController)
    {
        _terminalController = terminalController;
        RegisterCommands();
    }

    public void SetActive(bool isActive)
    {
        // _isActive = isActive;
    }

    private void RegisterCommands()
    {
        // _terminalController.RegisterCommand("ExampleCommand", ExampleCommand, "params: example");
    }
    //
    // private void ExampleCommand(string[] args)
    // {
    //     if (!_isActive)
    //         return;
    // }
}