using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace JogoSpaceInvaders
{
    public class Program
    {

        public static void Main()
        {
            Console.Clear();
            Console.CursorVisible = false;
            Console.SetWindowSize(80, 43);
            Jogo.Inicia();
        }
    }

    public class DronScouter : Inimigo
    {
        public int Direcao { get; set; }
        public DronScouter(int x, int y) : base(x, y)
        {
            Direcao = 1; // Inicialmente vai para a direita
        }

        public override void Draw()
        {
            if (X >= 0 && X < 80 && Y >= 0 && Y < 40)
            {
                Console.SetCursorPosition(X, Y);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("D");
            }
        }
        public override void Update()
        {
            // Atualiza a posição do drone
            base.Update();
            X += Direcao;

            if (X >= 78 || X <= 1)
            {
                Direcao *= -1;
            }
            Random random = new Random();

            if (random.Next(0, 100) > 98)
            {
                Jogo.DispararLaser(X, Y + 1, false, 0, 0);
            }
        }
    }


    public class GruntAlien : Inimigo
    {
        public GruntAlien(int x, int y) : base(x, y)
        {

        }

        public override void Draw()
        {
            if (X >= 0 && X < 80 && Y >= 0 && Y < 40)
            {
                Console.SetCursorPosition(X, Y);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("G");
            }
        }
        public override void Update() => base.Update();
    }

    public class ImperadorZodd : Inimigo
    {
        public ImperadorZodd(int x, int y) : base(x, y) { }

        public override void Draw()
        {
            if (X >= 0 && X < 80 && Y >= 0 && Y < 40)
            {
                Console.SetCursorPosition(X, Y);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Z");
            }
        }

        public override void Update()
        {
            base.Update();
            Random random = new Random();
            if (random.Next(1, 100) > 85)
            {
                var direcao = random.Next(0, 2);
                var cruzado = random.Next(0, 2);
                Jogo.DispararLaser(X, Y + 1, false, cruzado, direcao);
            }
        }
    }
    public class Inimigo
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int DescerCooldown { get; set; }
        public int DescerSpeed { get; set; }

        public Inimigo(int x, int y)
        {
            X = x;
            Y = y;
            DescerCooldown = 0;
            DescerSpeed = 50;
        }

        public virtual void Draw() { }

        public virtual void Update()
        {
            DescerCooldown++;
            if (DescerCooldown >= DescerSpeed)
            {
                Y++;
                DescerCooldown = 0;
            }
        }


    }
    public class Jogo
    {
        public static NaveJogador Jogador { get; set; } = new NaveJogador(40, 35);
        public static List<Inimigo> Inimigos { get; set; } = new List<Inimigo>();
        public static List<Laser> Lasers { get; set; } = new List<Laser>();
        public static List<PowerUp> PowerUp { get; set; } = new List<PowerUp>();
        public static int Pontuacao { get; set; }
        public static int FaseAtual { get; set; }
        public static bool JogoPausado { get; set; }
        public static int qtdPowerUp { get; set; }

        public static void Inicia()
        {
            Console.CursorVisible = false;
            InicializarJogo();
            FaseAtual = 1;
            qtdPowerUp = 0;
            while (true)
            {
                if (!JogoPausado)
                {
                    ExecutaAtualizacaoGame();
                }
                else
                {
                    DesenharPausa();
                }
                Thread.Sleep(100);
            }
        }

        public static void InicializarJogo()
        {
            Lasers.Clear();
            Inimigos.Clear();
            CarregarFase(1);
            Pontuacao = 0;
            JogoPausado = false;
        }

        public static void CarregarFase(int fase)
        {
            Random random = new Random();
            switch (fase)
            {
                case 1:
                    for (int i = 0; i < 20; i++)
                    {
                        int xPos = 15 + i % 5 * 8;
                        int yPos = 2 + i / 5 * 3;
                        Inimigos.Add(new GruntAlien(xPos, yPos));
                    }
                    PowerUp.Add(new PowerUp(random.Next(1, 78), random.Next(1, 38), random.Next(1, 3)));

                    break;
                case 2:
                    for (int i = 0; i < 15; i++)
                    {
                        int xPos = 15 + i % 5 * 8;
                        int yPos = 2 + i / 5 * 3;
                        Inimigos.Add(new DronScouter(xPos, yPos));
                    }
                    PowerUp.Add(new PowerUp(random.Next(1, 78), random.Next(1, 38), random.Next(1, 3)));
                    break;
                case 3:
                    for (int i = 0; i < 8; i++)
                    {
                        int xPos = 15 + (i % 4) * 10;
                        int yPos = 10 + (i / 4) * 4;
                        Inimigos.Add(new DronScouter(xPos, yPos));
                    }
                    PowerUp.Add(new PowerUp(random.Next(1, 78), random.Next(1, 38), random.Next(1, 3)));
                    Inimigos.Add(new Warship(35, 2));
                    Inimigos.Add(new ImperadorZodd(40, 2));
                    break;
            }
        }

        public static void ExecutaAtualizacaoGame()
        {
            Jogador.ExecutaAcoesPlayer();

            foreach (var laser in Lasers)
            {
                if (laser.player)
                {
                    laser.UpdateLaserPlayer();
                }
                else
                {
                    laser.UpdateLaserInimigo();
                }
            }

            foreach (var inimigo in Inimigos)
            {
                inimigo.Update();
            }

            VerificarColisoes();
            RenderizaInformacoes();
        }

        public static void VerificarColisoes()
        {
            foreach (var laser in Lasers.ToList())
            {
                foreach (var inimigo in Inimigos.ToList())
                {
                    if (laser.X == inimigo.X && laser.Y == inimigo.Y && laser.player)
                    {
                        Lasers.Remove(laser);
                        Inimigos.Remove(inimigo);
                        Console.Beep(500, 100);
                        Pontuacao += 100;
                    }
                }

                if (laser.X == Jogador.X && laser.Y == Jogador.Y && !laser.player)
                {
                    Lasers.Remove(laser);
                    Jogador.Vida = Jogador.Vida - 1;
                    if (Jogador.Vida == 0)
                    {
                        MostrarGameOver();
                    }
                }
            }

            foreach (var inimigo in Inimigos.ToList())
            {
                if (inimigo.X == Jogador.X && inimigo.Y == Jogador.Y)
                {
                    Inimigos.Remove(inimigo);
                    Jogador.Vida--;
                    if (Jogador.Vida == 0)
                    {
                        MostrarGameOver();
                    }
                }
            }
            foreach (var powerup in PowerUp.ToList())
            {
                if (powerup.X == Jogador.X && powerup.Y == Jogador.Y)
                {
                    PowerUp.Remove(powerup);
                    qtdPowerUp = qtdPowerUp + 1;
                    if (powerup.tipo == 1)
                    {
                        Jogador.Vida = Jogador.Vida + 2;
                    }
                    else if (powerup.tipo == 2)
                    {
                        Jogador.powerUps.Add(powerup);
                    }
                }
            }
            if (Inimigos.Count == 0)
            {
                if (FaseAtual == 3) // Fase final
                {
                    MostrarGameOver();
                    return;
                }
                FaseAtual++;
                CarregarFase(FaseAtual);
            }
        }

        public static void EmitirSomTiro()
        {
            Console.Beep(500, 100);
        }


        public static void RenderizaInformacoes()
        {
            Console.Clear();
            Jogador.Draw();

            foreach (var laser in Lasers)
            {
                laser.Draw();
            }

            foreach (var inimigo in Inimigos)
            {
                inimigo.Draw();
            }



            foreach (var powerup in PowerUp)
            {
                powerup.Draw();
            }
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" Vida: {Jogador.Vida} | Pontos: {Pontuacao} | Fase: {FaseAtual}");
            Console.SetCursorPosition(0, 40);
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.SetCursorPosition(0, 41);
            Console.WriteLine($"PowerUps: {qtdPowerUp}");
        }

        public static void PausarJogo()
        {
            JogoPausado = !JogoPausado;
        }

        public static void DesenharPausa()
        {
            Console.Clear();
            Console.SetCursorPosition(30, 15);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Jogo Pausado");
            Console.SetCursorPosition(30, 16);
            Console.WriteLine("Pressione qualquer tecla para continuar...");
        }

        public static void DispararLaser(int x, int y, bool player, int laserCruzado, int direcao)
        {
            Lasers.Add(new Laser(x, y, player, laserCruzado, direcao));
        }

        public static void MostrarGameOver()
        {
            Console.Clear();
            Console.SetCursorPosition(30, 15);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("GAME OVER!");
            Console.SetCursorPosition(30, 16);
            Console.WriteLine($"Pontos finais: {Pontuacao}");
            Console.SetCursorPosition(30, 17);
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
    public class Laser
    {
        public int X { get; set; }
        public int Y { get; set; }

        public bool player { get; set; }

        public int cruzado { get; set; }

        public int direcao { get; set; }

        public Laser(int x, int y, bool player, int cruzado, int direcao = 0)
        {
            X = x;
            Y = y;
            this.player = player;
            this.cruzado = cruzado;
            this.direcao = direcao;

        }

        public void UpdateLaserPlayer()
        {
            Y -= 1;  // O laser agora vai para cima
        }
        public void UpdateLaserInimigo()
        {
            Y += 1;  // O laser agora vai para cima
            if (cruzado == 1)
            {
                if (direcao == 0)
                    X -= 1;
                else
                {
                    X += 1;
                }
            }
        }
        public void Draw()
        {
            if (X >= 0 && X < 80 && Y >= 0 && Y < 40)
            {
                Console.SetCursorPosition(X, Y);
                if (player)
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                Console.Write("|");
            }
        }
    }

    public class NaveJogador
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Vida { get; set; }
        public int PowerUps { get; set; }
        private int TiroCooldown { get; set; }
        private Dictionary<ConsoleKey, int> cooldowns = new Dictionary<ConsoleKey, int>();
        private const int VelocidadeMovimento = 1;
        private const int LimiteSuperior = 4;
        private const int LimiteInferior = 36;
        private const int LimiteEsquerdo = 1;
        private const int LimiteDireito = 78;
        public List<PowerUp> powerUps = new List<PowerUp>();

        public NaveJogador(int x, int y)
        {
            X = x;
            Y = y;
            Vida = 3;
            PowerUps = 0;
            TiroCooldown = 5;
        }

        public void Draw()
        {
            if (X >= 0 && X < 80 && Y >= 0 && Y < 40)
            {
                Console.SetCursorPosition(X, Y);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("P");
            }
        }

        public void ExecutaAcoesPlayer()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true).Key;

                if (!cooldowns.ContainsKey(key) || cooldowns[key] == 0)
                {
                    if (key == ConsoleKey.UpArrow && Y > LimiteSuperior) { Y -= VelocidadeMovimento; cooldowns[key] = 2; };
                    if (key == ConsoleKey.DownArrow && Y < LimiteInferior) { Y += VelocidadeMovimento; cooldowns[key] = 2; };
                    if (key == ConsoleKey.LeftArrow && X > LimiteEsquerdo) { X -= VelocidadeMovimento; cooldowns[key] = 2; };
                    if (key == ConsoleKey.RightArrow && X < LimiteDireito) { X += VelocidadeMovimento; cooldowns[key] = 2; };

                    if (key == ConsoleKey.Spacebar)
                    {

                        var superTiroCount = 1;
                        foreach (var powerUp in powerUps)
                        {

                            if (powerUp.tipo == 2)
                            {
                                superTiroCount += 2;
                            }
                        }

                        for (int i = 0; i < superTiroCount; i++)
                        {
                            Jogo.DispararLaser(X, Y - i, true, 0, 0);
                            Jogo.EmitirSomTiro();
                        }
                        cooldowns[key] = 7;
                    }

                    if (key == ConsoleKey.Escape)
                    {
                        Jogo.PausarJogo();
                    }

                    // Define um cooldown de 100 milissegundos para a tecla pressionada
                }
            }

            // Atualiza os cooldowns
            foreach (var key in cooldowns.Keys.ToList())
            {
                if (cooldowns[key] > 0)
                {
                    cooldowns[key]--;
                }
            }

            if (TiroCooldown > 0) TiroCooldown--;
        }
    }

    public class PowerUp
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int tipo { get; set; }
        public PowerUp(int x, int y, int tipo)
        {
            X = x;
            Y = y;
            this.tipo = tipo;
        }

        public virtual void Draw()
        {

            Console.SetCursorPosition(X, Y);

            if (tipo == 1)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("H");
            }
            else if (tipo == 2)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("S");
            }
        }
    }

    public class Warship : Inimigo
    {
        public Warship(int x, int y) : base(x, y) { }

        public override void Draw()
        {
            if (X >= 0 && X < 80 && Y >= 0 && Y < 40)
            {
                Console.SetCursorPosition(X, Y);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("W");
            }
        }

        public override void Update()
        {
            base.Update();
            Random random = new Random();
            if (random.Next(1, 100) > 90)
            {
                var direcao = random.Next(0, 2);
                Jogo.DispararLaser(X, Y + 1, false, 1, direcao);
            }
        }
    }
}