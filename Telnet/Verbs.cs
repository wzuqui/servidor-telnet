namespace ConsoleAppAsync.Telnet
{
  public enum Verbs
  {
    UNKNOWN = -1,
    SGA = 3,
    WILL = 251,
    WONT = 252,
    DO = 253,
    DONT = 254,
    IAC = 255
  }
}
