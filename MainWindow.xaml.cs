using org.mariuszgromada.math.mxparser;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Expression = org.mariuszgromada.math.mxparser.Expression;

namespace CoordinateDescentMethod
{
    public class FunctionModel
    {
        private static readonly double PHI = (1 + Math.Sqrt(5)) / 2;

        public static (double, double, double) CoordinateDescentMethod(string functionExpression, double parametrA, double parametrB, double epsilon, bool isMinimumSearched)
        {
            double middlePoint = (parametrA + parametrB) / 2;
            double pointX, pointY, pointZ;
            double nextPointX = middlePoint;
            double nextPointY = middlePoint;
            double nextPointZ = middlePoint;

            do
            {
                pointX = nextPointX;
                pointY = nextPointY;
                pointZ = nextPointZ;

                string functionExpressionAccordingX = functionExpression.Replace("y", pointY.ToString());
                functionExpressionAccordingX = functionExpressionAccordingX.Replace("z", pointZ.ToString());
                Function expressionAccordingX = ConvertExpressionToFunctionFromString(functionExpressionAccordingX.Replace(",", "."));
                nextPointX = FindExtremumByGoldenSection(expressionAccordingX, parametrA, parametrB, epsilon);

                string functionExpressionAccordingY = functionExpression.Replace("x", pointX.ToString());
                functionExpressionAccordingY = functionExpressionAccordingY.Replace("z", pointZ.ToString());
                functionExpressionAccordingY = functionExpressionAccordingY.Replace("y", "x");
                Function expressionAccordingY = ConvertExpressionToFunctionFromString(functionExpressionAccordingY.Replace(",", "."));
                nextPointY = FindExtremumByGoldenSection(expressionAccordingY, parametrA, parametrB, epsilon);

                string functionExpressionAccordingZ = functionExpression.Replace("x", pointX.ToString());
                functionExpressionAccordingZ = functionExpressionAccordingZ.Replace("y", pointY.ToString());
                functionExpressionAccordingZ = functionExpressionAccordingZ.Replace("z", "x");
                Function expressionAccordingZ = ConvertExpressionToFunctionFromString(functionExpressionAccordingZ.Replace(",", "."));
                nextPointZ = FindExtremumByGoldenSection(expressionAccordingZ, parametrA, parametrB, epsilon);
            } while (Math.Abs(nextPointX - pointX) > epsilon || Math.Abs(nextPointY - pointY) > epsilon || Math.Abs(nextPointZ - pointZ) > epsilon);

            return (nextPointX, nextPointY, nextPointZ);
        }

        //  Поиск точки минимума методом золотого сечения 
        public static double FindExtremumByGoldenSection(Function expression, double parametrA, double parametrB, double epsilon)
        {
            do
            {
                double firstDot = parametrB - (parametrB - parametrA) / PHI;
                double secondDot = parametrA + (parametrB - parametrA) / PHI;
                if (SolveFunc(expression, firstDot) >= SolveFunc(expression, secondDot))
                {
                    parametrA = firstDot;
                } else
                {
                    parametrB = secondDot;
                }
            } while (Math.Abs(parametrB - parametrA) > epsilon);
            return (parametrA + parametrB) / 2;
        }

        //  Метод для вычисления значения функции в точке x
        public static double SolveFunc(Function function, double x)
        {
            return new Expression($"f({x.ToString().Replace(",", ".")})", function).calculate();
        }

        // конвертирует выражение из типа строка в тип Function
        public static Function ConvertExpressionToFunctionFromString(string functionExpression)
        {
            return new Function("f(x) = " + functionExpression);
        }
    }

    public class FunctionViewModel : INotifyPropertyChanged
    {
        private string functionExpression;
        private double parametrA;
        private double parametrB;
        private string resultText = "Результат: ";
        private double epsilon;

        public string FunctionExpression
        {
            get => functionExpression;
            set
            {
                functionExpression = value.ToLower();
                OnPropertyChanged(nameof(FunctionExpression));
            }
        }

        public double ParametrA
        {
            get => parametrA;
            set
            {
                parametrA = value;
                OnPropertyChanged(nameof(ParametrA));
            }
        }

        public double ParametrB
        {
            get => parametrB;
            set
            {
                parametrB = value;
                OnPropertyChanged(nameof(ParametrB));
            }
        }

        public double Epsilon
        {
            get => epsilon;
            set
            {
                epsilon = value;
                OnPropertyChanged(nameof(Epsilon));
            }
        }

        public string ResultText
        {
            get => resultText;
            set
            {
                resultText = value;
                OnPropertyChanged(nameof(ResultText));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand FindLocalExtremumCommand { get; }

        public FunctionViewModel()
        {
            FindLocalExtremumCommand = new RelayCommand(_ => FindLocalExtremum());
        }

        private void FindLocalExtremum()
        {
            (double, double, double) localMinimum = FunctionModel.CoordinateDescentMethod(FunctionExpression, ParametrA, ParametrB, Epsilon, true);
            (double, double, double) localMaximum = FunctionModel.CoordinateDescentMethod(FunctionExpression, ParametrA, ParametrB, Epsilon, false);
            ResultText = $"Результат:\nМинимум: x = {Math.Round(localMinimum.Item1, 2, MidpointRounding.AwayFromZero)}, y = {Math.Round(localMinimum.Item2, 2, MidpointRounding.AwayFromZero)}, z = {Math.Round(localMinimum.Item3, 2, MidpointRounding.AwayFromZero)}";
                         // $"Максимум: x = {Math.Round(localMaximum.Item1, 2, MidpointRounding.AwayFromZero)}, y = {Math.Round(localMaximum.Item2, 2, MidpointRounding.AwayFromZero)}, z = {Math.Round(localMaximum.Item3, 2, MidpointRounding.AwayFromZero)}";
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {   
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new FunctionViewModel();
        }
    }
}
