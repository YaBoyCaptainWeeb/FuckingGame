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
        public Player player2 = new Player(2, "Боба", new BitmapImage(new Uri("pack://application:,,,/ASSets/Player2.png")), "Ожидает"); // Сделать ViewModel, без него никуда, походу
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
        public List<Unit> GenerateUnits()
        {
            List<Unit> result = new List<Unit>();
            result.Add(new Swordsman(1,player1, 100, 50, 3, 2,XSize,YSize));
            result.Add(new Swordsman(3,player1, 100, 50,0,0,XSize,YSize));
            result.Add(new Swordsman(2,player2, 100, 50, 1, 1,XSize,YSize));
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
                rect.Stretch = Stretch.Uniform;
                rect.MouseUp += new MouseButtonEventHandler(MouseBtn);

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
                Image un = new Image();
                un.Width = unit.XSize;
                un.Height = unit.YSize;
                un.Stretch = Stretch.Uniform;
                if (unit.Player == player2)
                {
                    unit.source = new BitmapImage(new Uri("ASSets/Swordsman_enemy.png", UriKind.Relative));
                } 
                un.Source = unit.source;
                un.MouseUp += new MouseButtonEventHandler(MouseBtn1);
                CanvasMap.Children.Add(un);
                Canvas.SetZIndex(un, 2);
                Canvas.SetLeft(un, (unit.X * unit.XSize));
                Canvas.SetTop(un, (unit.Y * unit.YSize));
            }
        }
        private void MouseBtn(object sender, MouseEventArgs e)
        {
            Point pnt = new Point(Canvas.GetLeft(sender as FrameworkElement),Canvas.GetTop(sender as FrameworkElement));
            MessageBox.Show($"MousePos: {pnt.X},{pnt.Y}; Coord?: {Math.Ceiling(pnt.X/XSize)},{Math.Ceiling(pnt.Y/YSize)}");
            int XMove = Convert.ToInt32(Math.Ceiling(pnt.X / XSize));
            int YMove = Convert.ToInt32(Math.Ceiling(pnt.Y / YSize));
            if (savedSelectedUnit != null) // Логика перемещений тоже почти готова, инкапсулировать и доделать потом
            {
                controller.MoveUnit(savedSelectedUnit,XMove,YMove);
                CanvasMap.Children.Clear();
                RenderMap(controller._map.Tiles);
                RenderUnits(controller._map.Units);
            }
        }
        private void MouseBtn1(object sender, MouseEventArgs e)
        {
            Point pnt = new Point(Canvas.GetLeft(sender as FrameworkElement),Canvas.GetTop(sender as FrameworkElement));
            selectedUnit = controller._map.Units.Where(x => x.X == (pnt.X / XSize) && x.Y == (pnt.Y / YSize) && x.isDead == false).FirstOrDefault();
            
            if (savedSelectedUnit == null && selectedUnit.Player == currentMove) // Логика почти готова, позже инкапсулировать ее в каком-нибудь классе
            {
                savedSelectedUnit = selectedUnit;
                MessageBox.Show($"CellPos: {pnt.X},{pnt.Y}; UnitPos: {Math.Ceiling(pnt.X / XSize)},{Math.Ceiling(pnt.Y / YSize)}\n" +
                $"SavedSelected unit: {savedSelectedUnit.Id},{savedSelectedUnit.GetType().Name},Who owns: {savedSelectedUnit.Player.Name}");
            }
            else if (savedSelectedUnit == null)
            {
                MessageBox.Show("Данного юнита выбрать нельзя");
            } else if (savedSelectedUnit.Player == selectedUnit.Player) 
            {
                savedSelectedUnit = selectedUnit;
                MessageBox.Show($"New savedSelectedUnit: {savedSelectedUnit.Id},{savedSelectedUnit.GetType().Name},Who owns: {savedSelectedUnit.Player.Name}");
            } else if (selectedUnit.Player != savedSelectedUnit.Player) 
            {
                bool can = controller.CanAttackUnit(savedSelectedUnit, selectedUnit);

                MessageBox.Show("Данного юнита можно попытаться атаковать\n"+
                    $"Атакуемый Unit: {selectedUnit.GetType().Name}, Who owns: {selectedUnit.Player.Name}, CanAttack?: {can}");
            }
            //if (selectedUnit != null)
            //{
            //    
            //}
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