using System.Windows;
using TangerineAutomationSystem.Models;

namespace TangerineAutomationSystem.Services
{
    // 模块提供器接口，外部实现可以提供自定义编辑器视图（FrameworkElement）
    public interface IModuleProvider
    {
        string Name { get; }
        string Description { get; }
        FrameworkElement? GetEditor(ModuleModel module);
    }
}