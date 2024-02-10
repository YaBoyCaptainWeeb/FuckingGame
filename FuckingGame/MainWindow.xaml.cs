using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Strategy.Domain;
using Strategy.Domain.Models;


namespace FuckingGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameController controller = null!; // контроллер
        public Player player1 = new Player(1, "Биба", new BitmapImage(new Uri("pack://application:,,,/ASSets/Player1.png")), "Ходит");
        public Player player2 = new Player(2, "Боба", new BitmapImage(new Uri("pack://application:,,,/ASSets/Player2.png")), "Ожидает");
        public Player currentMove;
        public Unit selectedUnit, savedSelectedUnit;

        ViewModel vm = new ViewModel();
        
        int XSize = 64, YSize = 64;
        int RectCounter = 0;


        private readonly int[,] _map = // Примитивная карта для рендера 0 - земля, 1 - вода
        {
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 },
            {0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0 }
        };
        public MainWindow()
        {
            InitializeComponent();
        }

        private List<Tile> GenerateTiles(int[,] map,int TileSizeX,int TileSizeY) // Заполнение класса Map тайлами
        {
            List<Tile> result = new List<Tile>();
            int rows = map.GetUpperBound(0) + 1;
            int cols = map.Length / rows;
            for (int i = 0; i != rows; i++)
            {
                for (int j = 0; j != cols; j++)
                {
                    switch (map[i, j])
                    {
                        case 0:
                            result.Add(new Grass(j, i, TileSizeX,TileSizeY));
                            break;
                        case 1:
                            result.Add(new Water(j, i, TileSizeX, TileSizeY));
                            break;
                    }
                }
            }
            return result;
        }
        public List<Unit> GenerateUnits() // подумать над тем, как будут добавляться юниты
        {
            List<Unit> result = new List<Unit>();
            result.Add(new Swordsman(1,player1, 100, 50, 5, 6,XSize,YSize));
            result.Add(new Swordsman(2,player1, 100, 50,5,7,XSize,YSize));
            result.Add(new Swordsman(3,player2, 100, 50, 15, 6,XSize,YSize));
            result.Add(new Swordsman(4,player2,100,50,15,7,XSize,YSize));

            result.Add(new Archer(5, player1, 50, 50, 6, 6, XSize, YSize));
            result.Add(new Archer(6, player2, 50, 50,14,6,XSize,YSize));

            result.Add(new Horseman(7,player1,200,75,5,5,XSize,YSize));
            result.Add(new Horseman(8,player2,200,75,15,5,XSize,YSize));

            result.Add(new Catapult(9,player1,75,100,4,6,XSize,YSize));
            result.Add(new Catapult(10,player2,75,100,16,6,XSize,YSize));
            player1.OwnedUnits.Add(result[0]);
            player1.OwnedUnits.Add(result[1]);
            player1.OwnedUnits.Add(result[4]);
            player1.OwnedUnits.Add(result[6]);
            player2.OwnedUnits.Add(result[2]);
            player2.OwnedUnits.Add(result[3]);
            player2.OwnedUnits.Add(result[5]);
            player2.OwnedUnits.Add(result[7]);
            player1.OwnedUnits.Add(result[8]);
            player2.OwnedUnits.Add(result[9]);
            return result;
        }
        private void RenderMap(IReadOnlyList<Tile> map)
        {
            int stepX = 0;
            int stepY = 0;

            int _mapRows = _map.GetUpperBound(0) + 1;
            int _mapCols = _map.Length / _mapRows;
            CanvasMap.Width = (map[0].XSize * _mapCols);
            CanvasMap.Height = (map[0].YSize * _mapRows);

            foreach(Tile tile in map)
            {
                Image rect = new Image();
                rect.Tag = ++RectCounter;
                rect.Width = tile.XSize; 
                rect.Height = tile.YSize;
                rect.Stretch = Stretch.UniformToFill;

                switch (tile)
                {
                    case Grass:
                        rect.Source = tile.source;
                        break;
                    case Water:
                        rect.Source = tile.source;
                        break;
                }
                CanvasMap.Children.Add(rect);
                Canvas.SetZIndex(rect, 0);

                if (Convert.ToInt32(rect.Tag) % _mapCols == 0)
                {
                    Canvas.SetLeft(rect, stepX);
                    Canvas.SetTop(rect, stepY);
                    stepX = 0;
                    stepY += tile.YSize;
                }
                else
                {
                    Canvas.SetLeft(rect, stepX);
                    Canvas.SetTop(rect, stepY);
                    stepX += tile.XSize;
                }
            }
        }
        private void RenderUnits(IReadOnlyList<Unit> units)
        {
            foreach (Unit unit in units)
            {
                UnitTemplate un = new UnitTemplate();
                un.UnitImage.Width = unit.XSize;
                un.UnitImage.Height = unit.YSize;
                un.UnitImage.Stretch = Stretch.UniformToFill;
                un.UnitHealthBar.Width = unit.XSize;
                un.UnitHealthBar.Maximum = unit.MaxHP;
                un.UnitHealthBar.Value = unit.hp;
                if (unit.Player == player2 && unit.isDead == false)
                {
                    un.UnitImage.Source = unit.enemySource;
                } else if(unit.isDead == true)
                {
                    un.UnitImage.Source = unit.source;
                } else
                {
                    un.UnitImage.Source = unit.source;
                }                
                un.MouseUp += new MouseButtonEventHandler(MouseBtn1);
                CanvasMap.Children.Add(un);
                if (unit.isDead == true)
                {
                    Canvas.SetZIndex(un, 1);
                } else
                {
                    Canvas.SetZIndex(un, 2);
                }
                Canvas.SetLeft(un, (unit.X * unit.XSize));
                Canvas.SetTop(un, (unit.Y * unit.YSize));
            }
        }
        private void MouseBtn1(object sender, MouseEventArgs e)
        {
            Point pnt = new Point(Canvas.GetLeft(sender as FrameworkElement),Canvas.GetTop(sender as FrameworkElement));
            selectedUnit = controller._map.Units.Where(x => x.X == (pnt.X / XSize) && x.Y == (pnt.Y / YSize) && x.isDead == false).FirstOrDefault();

            if (savedSelectedUnit == null && selectedUnit.Player == currentMove) // Логика почти готова, позже инкапсулировать ее в каком-нибудь классе
            {
                savedSelectedUnit = selectedUnit;

                CreateHighlight(savedSelectedUnit,player1,player2,(int)pnt.X, (int)pnt.Y);
                
                //MessageBox.Show($"CellPos: {pnt.X},{pnt.Y}; UnitPos: {Math.Ceiling(pnt.X / XSize)},{Math.Ceiling(pnt.Y / YSize)}\n" +
                //$"SavedSelected unit: {savedSelectedUnit.Id},{savedSelectedUnit.GetType().Name},Who owns: {savedSelectedUnit.Player.Name}");
            } else if (savedSelectedUnit != null && selectedUnit == null)
            {
                MoveUnit((int)(pnt.X / XSize), (int)(pnt.Y / YSize));
            }
            else if (savedSelectedUnit == null)
            {
                MessageBox.Show("Данного юнита выбрать нельзя");
            } else if (savedSelectedUnit.Player == selectedUnit.Player) 
            {
                savedSelectedUnit = selectedUnit;

                CanvasMap.Children.Clear();
                RenderMap(controller._map.Tiles);
                RenderUnits(controller._map.Units);
                CreateHighlight(savedSelectedUnit, player1, player2, (int)pnt.X, (int)pnt.Y);

                //MessageBox.Show($"New savedSelectedUnit: {savedSelectedUnit.Id},{savedSelectedUnit.GetType().Name},Who owns: {savedSelectedUnit.Player.Name}");
            } else if (selectedUnit.Player != savedSelectedUnit.Player) 
            {
                bool can = controller.CanAttackUnit(savedSelectedUnit, selectedUnit);

                //MessageBox.Show("Данного юнита можно попытаться атаковать\n"+
                //    $"Атакуемый Unit: {selectedUnit.GetType().Name}, Who owns: {selectedUnit.Player.Name}, CanAttack?: {can}, HP Before Attack: {selectedUnit.hp}");
                if (can)
                {
                    controller.AttackUnit(savedSelectedUnit, selectedUnit);
                    //MessageBox.Show("Атака произошла\n" +
                    // $"Атакуемый Unit: {selectedUnit.GetType().Name}, Who owns: {selectedUnit.Player.Name}, CanAttack?: {can}, HP After Attack: {selectedUnit.hp}");                    
                    EndTurn();
                } else
                {
                    MessageBox.Show("Выбранная цель находится вне радиуса атаки вашего юнита");
                }
            }
        }

        private void CreateHighlight(Unit unit,Player _player1,Player _player2,int X,int Y) // создает подсветку для тайлов
        {

            Rectangle rect = new Rectangle() // Индивидуальная подсветка для выбранного юнита
            {
                Width = XSize,
                Height = YSize,
                Fill = Brushes.White,
                Opacity = 0.6,
            };
            CanvasMap.Children.Add(rect);
            Canvas.SetZIndex(rect, 1);
            Canvas.SetLeft(rect, X);
            Canvas.SetTop(rect, Y);
            
            foreach (Tile tile in controller._map.Tiles)
            {
                if (tile is not Water)
                {
                    if (controller.CanMoveUnit(unit, tile.X, tile.Y))
                    {
                        rect = new Rectangle()
                        {
                            Width = XSize,
                            Height = YSize,
                            Fill = Brushes.BlueViolet,
                            Opacity = 0.3
                        };
                        rect.MouseUp += new MouseButtonEventHandler(MouseBtn1);
                        CanvasMap.Children.Add(rect);
                        Canvas.SetZIndex(rect, 1);
                        Canvas.SetLeft(rect, tile.X * tile.XSize);
                        Canvas.SetTop(rect, tile.Y * tile.YSize);
                    }
                }
            }
            CreateUnitHighlight(_player1, unit);
            CreateUnitHighlight(_player2, unit);
        }

        private void CreateUnitHighlight(Player player, Unit unit)
        {
            Rectangle rect = new Rectangle();
            foreach (Unit collision in player.OwnedUnits)
            {
                if (unit.Player == player && unit != collision && collision.isDead == false)
                {
                    rect = new Rectangle()
                    {
                        Width = XSize,
                        Height = YSize,
                        Fill = Brushes.Green,
                        Opacity = 0.3
                    };
                    rect.MouseUp += new MouseButtonEventHandler(MouseBtn1);
                    CanvasMap.Children.Add(rect);
                    Canvas.SetZIndex(rect, 1);
                    Canvas.SetLeft(rect, collision.X * collision.XSize);
                    Canvas.SetTop(rect, collision.Y * collision.YSize);
                }
                else if (unit != collision && collision.isDead == false && unit.CanAttack(collision))
                {
                    rect = new Rectangle()
                    {
                        Width = XSize,
                        Height = YSize,
                        Fill = Brushes.Red,
                        Opacity = 0.3
                    };
                    rect.MouseUp += new MouseButtonEventHandler(MouseBtn1);
                    CanvasMap.Children.Add(rect);
                    Canvas.SetZIndex(rect, 1);
                    Canvas.SetLeft(rect, collision.X * collision.XSize);
                    Canvas.SetTop(rect, collision.Y * collision.YSize);
                }
            }
        } // Создает подсветку конкретного юнита
        private void MoveUnit(int XMove,int YMove)
        {
            if (savedSelectedUnit != null)
            {
                if (controller.CanMoveUnit(savedSelectedUnit, XMove, YMove))
                {
                    controller.MoveUnit(savedSelectedUnit, XMove, YMove);
                    EndTurn();
                }
            }
        }
        
        private void EndTurn()
        {
            currentMove = controller.ChangeMove(player1, player2);
            if (currentMove.OwnedUnits.Where(x => x.isDead == false).FirstOrDefault() != null)
            {
                savedSelectedUnit = null;
                StatusText.Text = player1.Status;
                StatusText1.Text = player2.Status;
                CanvasMap.Children.Clear();
                RenderMap(controller._map.Tiles);
                RenderUnits(controller._map.Units);
            } else
            {
                MessageBox.Show($"Игрок {currentMove.Name} потерял всех юнитов, игра окончена!");
                Application.Current.Shutdown();
            }

        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            controller = new GameController(new Map(GenerateTiles(_map, XSize, YSize),GenerateUnits()));
            currentMove = player1;

            vm.player1 = player1;
            vm.player2 = player2;

            Player1Panel.DataContext = vm;
            Player2Panel.DataContext = vm;
            RenderMap(controller._map.Tiles);
            RenderUnits(controller._map.Units);
        }
    }
}