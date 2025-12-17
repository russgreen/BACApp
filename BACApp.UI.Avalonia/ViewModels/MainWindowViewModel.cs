using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACApp.UI.Avalonia.ViewModels;

internal class MainWindowViewModel : BaseViewModel
{
    private readonly ILogger<MainWindowViewModel> _logger;

    public string WindowTitle { get; private set; }

    public MainWindowViewModel()
    {
        _logger = null;
    }


    public MainWindowViewModel(ILogger<MainWindowViewModel> logger)
    {
        _logger = logger;

        this.WindowTitle = "BAC Cloudbase App";

        _logger.LogDebug(WindowTitle);
    }
}
