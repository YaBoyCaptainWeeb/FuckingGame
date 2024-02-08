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
//using System.Drawing;
using Strategy.Domain;
using Strategy.Domain.Models;


namespace FuckingGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameController controller; // контроллер
        Player player1 = new Player(1,"Биба", new BitmapImage(new Uri("ASSets/Player1.png",UriKind.Relative))); // Доделать игроков и ходы
        Player player2 = new Player(2, "Боба", new BitmapImage(new Uri("ASSets/Player2.png", UriKind.Relative))); // Сделать ViewModel, без него никуда, походу
        
       

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
            Player pl1 = player1;

            InitializeComponent();

            img1.DataContext = pl1.Portrait;
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
                Canvas.SetZIndex(rect, 0);

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

                if (Convert.ToInt32(rect.Tag) % 20 == 0)
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
        private void MouseBtn(object sender, MouseEventArgs e)
        {
            Point pnt = new Point(Canvas.GetLeft(sender as FrameworkElement),Canvas.GetTop(sender as FrameworkElement));
            MessageBox.Show($"Mouse/rect pos: {pnt.X},{pnt.Y}, RectCount: {(sender as FrameworkElement).Tag}");
        }
        private void CanvasMap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = Mouse.GetPosition(sender as Canvas);
            Coordinates crd = new Coordinates(Convert.ToInt32(Math.Ceiling(pnt.X / XSize))-1,Convert.ToInt32(Math.Ceiling(pnt.Y/YSize))-1);

            MessageBox.Show($"Mouse: {pnt.X},{pnt.Y}; Tile coord: {crd.X},{crd.Y}");
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            controller = new GameController(new Map(GenerateTiles(_map,XSize,YSize),null));
            RenderMap(controller._map.Tiles);
        }
    }
}