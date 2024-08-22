namespace LithRageTexPacker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: LithRAGETexUnpack.exe <path to 256 color bmp> <path to palette>");
                return;
            }
            else
            {
                string fileName = Path.GetFileNameWithoutExtension(args[0]);
                fileName = fileName.ToUpper();
                //TextureLoader.LoadTexture(args[0], args[1]);
                TextureSaver.SaveTextureFromBmp(args[0], args[1], $"{fileName}.BIN");
                
                return;
            }
        }
    }
}
