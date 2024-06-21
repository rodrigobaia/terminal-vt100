using System.Text;

namespace TerminalVT100
{
    /// <summary>
    /// Estado de digitação do Terminal
    /// </summary>
    public class ClientState
    {
        /// <summary>
        /// Linha do Cursor do Terminal
        /// </summary>
        public int Row { get; set; } = 1;

        /// <summary>
        /// Coluna do Cursor do Terminal
        /// </summary>
        public int Column { get; set; } = 1;

        /// <summary>
        /// Message do Terminal
        /// </summary>
        public StringBuilder MessageBuilder { get; set; } = new StringBuilder();
    }
}
