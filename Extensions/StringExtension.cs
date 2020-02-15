namespace ConsoleAppAsync.Extensions
{
  public static class StringExtension
  {
    public static bool Contains(this string texto, params char[] caracteres)
    {
      foreach (var caracter in caracteres)
        if (texto.Contains(caracter))
          return true;
      return false;
    }
  }
}
