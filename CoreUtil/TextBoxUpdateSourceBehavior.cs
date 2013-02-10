using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Core.Wpf
{
    // Using the TextBoxUpdateSourceBehavior
    // xmlns:util="clr-namespace:Core.Wpf"
    // <TextBox Text="{Binding Username}" util:TextBoxUpdateSourceBehaviour.BindingSource="Username" />
    //
    public class TextBoxUpdateSourceBehaviour
    {
        private static Dictionary<TextBox, PropertyInfo> _boundProperties = new Dictionary<TextBox, PropertyInfo>();

        public static readonly DependencyProperty BindingSourceProperty =
            DependencyProperty.RegisterAttached(
            "BindingSource",
            typeof(string),
            typeof(TextBoxUpdateSourceBehaviour),
            new PropertyMetadata(default(string), OnBindingChanged));

        public static void SetBindingSource(TextBox element, string value)
        {
            element.SetValue(BindingSourceProperty, value);
        }

        public static string GetBindingSource(TextBox element)
        {
            return (string)element.GetValue(BindingSourceProperty);
        }

        private static void OnBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var txtBox = d as TextBox;
            if (txtBox != null)
            {
                txtBox.Loaded += OnLoaded;
                txtBox.TextChanged += OnTextChanged;
            }
        }

        static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var txtBox = sender as TextBox;
            if (txtBox != null)
            {

                // Reflect the datacontext of the textbox to find the field to bind to.
                if (txtBox.DataContext != null) // DataContext is null in the designer
                {
                    var dataContextType = txtBox.DataContext.GetType();
                    AddToBoundPropertyDictionary(txtBox, dataContextType.GetRuntimeProperty(GetBindingSource(txtBox)));
                }

                // If you want the behaviour to handle your binding as well, uncomment the following.
                //var binding = new Binding();
                //binding.Mode = BindingMode.TwoWay;
                //binding.Path = new PropertyPath(GetBindingSource(txt));
                //binding.Source = txt.DataContext;
                //BindingOperations.SetBinding(txt, TextBox.TextProperty, binding);
            }
        }

        static void AddToBoundPropertyDictionary(TextBox txtBox, PropertyInfo boundProperty)
        {
            PropertyInfo propInfo;
            if (!_boundProperties.TryGetValue(txtBox, out propInfo))
            {
                _boundProperties.Add(txtBox, boundProperty);
            }
        }

        static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var txtBox = sender as TextBox;
            if (txtBox != null)
            {
                _boundProperties[txtBox].SetValue(txtBox.DataContext, txtBox.Text);

                //var data = _boundProperties[txtBox].GetValue(txtBox.DataContext);

                //if (data == null || (data != null && !data.Equals(txtBox.Text)))
                //{
                //    _boundProperties[txtBox].SetValue(txtBox.DataContext, txtBox.Text);
                //}
            }
        }
    }
}
