﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POS_UI.View
{
    /// <summary>
    /// Interaction logic for SetCouponDialog.xaml
    /// </summary>
    public partial class SetCouponDialog : UserControl
    {
        public SetCouponDialog()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var couponCode = CouponTextBox.Text?.Trim();
            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(couponCode, null);
        }
    }
}
