using Finanse.DataAccessLayer;
using Finanse.Models;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace Finanse.Pages {

    public sealed partial class Statystyki : Page {

        public Statystyki() {

            this.InitializeComponent();

            List<OperationCategory> lista = Dal.getAllCategories();

            (ColumnChart.Series[0] as ColumnSeries).ItemsSource = lista;
        }
    }
}
