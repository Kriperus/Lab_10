namespace PascalCompiler;


/// <summary>
/// Описание одной ошибки компилятора
/// </summary>
public class CompilerError
    {
        public ErrorCode Code { get; set; }
        
        public string Message { get; set; }
        
        public int Line { get; set; }
        
        public int Column { get; set; }
        
        public int ErrCount { get; set; }
    }
