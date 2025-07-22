using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace POS_UI.View
{
    public partial class SwipeToDeleteOrderItem : UserControl
    {
        public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(
            nameof(RemoveCommand), typeof(ICommand), typeof(SwipeToDeleteOrderItem), new PropertyMetadata(null));

        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        private Point _startPoint;
        private bool _isDragging;
        private double _dragThreshold = 160; // pixels to trigger delete
        private double _maxDrag = 180; // max drag distance

        public SwipeToDeleteOrderItem()
        {
            InitializeComponent();
            ContentBorder.MouseLeftButtonDown += ContentBorder_MouseLeftButtonDown;
            ContentBorder.MouseMove += ContentBorder_MouseMove;
            ContentBorder.MouseLeftButtonUp += ContentBorder_MouseLeftButtonUp;
            ContentBorder.MouseLeave += ContentBorder_MouseLeave;
        }

        private void ContentBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Stop any running animation and reset position if needed
            ContentTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, null);
            ContentTransform.X = 0;
            DeleteFeedback.Visibility = Visibility.Collapsed;
            _startPoint = e.GetPosition(this);
            _isDragging = true;
            ContentBorder.CaptureMouse();
        }

        private void ContentBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;
            var pos = e.GetPosition(this);
            var dx = pos.X - _startPoint.X;
            if (dx < 0) // Only allow left drag
            {
                double drag = Math.Max(dx, -_maxDrag);
                ContentTransform.X = drag;
                DeleteFeedback.Visibility = Visibility.Visible;
                if (Math.Abs(drag) > _dragThreshold)
                {
                    // Perform delete operation
                    DeleteOrderItem();
                    // Immediately end drag so user can start again
                    _isDragging = false;
                    ContentBorder.ReleaseMouseCapture();
                    DeleteFeedback.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                DeleteFeedback.Visibility = Visibility.Collapsed;
            }
        }

        private void ContentBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging) { EndDrag(); return; }
            // Only trigger if click is not on a button (quantity +/-)
            if (e.OriginalSource is DependencyObject depObj)
            {
                var parentButton = FindParent<Button>(depObj);
                if (parentButton != null)
                {
                    // Click was on a button, do not trigger edit
                    return;
                }
            }
            // Try to invoke EditOrderItemCommand on DataContext
            var orderItem = DataContext;
            var fe = this.TemplatedParent as FrameworkElement ?? this.Parent as FrameworkElement;
            var dc = fe?.DataContext ?? this.DataContext;
            var commandProp = dc?.GetType().GetProperty("EditOrderItemCommand");
            var command = commandProp?.GetValue(dc) as ICommand;
            if (command != null && command.CanExecute(orderItem))
            {
                command.Execute(orderItem);
            }
        }

        private void ContentBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isDragging)
                EndDrag();
        }

        private void EndDrag()
        {
            _isDragging = false;
            ContentBorder.ReleaseMouseCapture();
            AnimateContent(0);
            DeleteFeedback.Visibility = Visibility.Collapsed;
        }

        private void AnimateContent(double toX)
        {
            var anim = new DoubleAnimation
            {
                To = toX,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            ContentTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, anim);
        }

        private void DeleteOrderItem()
        {
            if (RemoveCommand != null && RemoveCommand.CanExecute(DataContext))
            {
                RemoveCommand.Execute(DataContext);
            }
            AnimateContent(-ActualWidth);
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindParent<T>(parentObject);
        }
    }
} 