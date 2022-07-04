using System;
using System.Collections.Generic;
using System.IO;
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
using VMS.TPS.Common.Model.API;

namespace StructureMargins
{
    /// <summary>
    /// Interaction logic for MarginsWindow.xaml
    /// </summary>
    public partial class MarginsWindow : Window
    {
        public ScriptContext scriptcontext;
        public List<DataGrid1Structure> DataGrid1StructureList = new List<DataGrid1Structure>() { };
        public ListCollectionView DataGrid1StructureCollection { get; set; }

        public List<DataGrid2Structure> DataGrid2StructureList = new List<DataGrid2Structure>() { };
        public ListCollectionView DataGrid2StructureCollection { get; set; }

        public List<Structure> ListOfCreatedTempStructure = new List<Structure>() { };

        public MarginsWindow(ScriptContext scriptContext)
        {
            this.scriptcontext = scriptContext;
            InitializeComponent();
            PopulateComboBoxDicomType();
            InitializeStructureListView();
            UpdateStructureListView();
            InitializeDataGrid2Structure();
            EnableCheckBoxAllowConversionHighResSegment();
        }

        public class DataGrid1Structure
        {
            public string Structure { get; set; }
            public string DicomType { get; set; }
            public bool IsHighRes { get; set; }
            public string ColorSet { get; set; }
        }

        public class DataGrid2Structure
        {
            public string OperationColor { get; set; }
            public string Operation { get; set; }
            public string OperationType { get; set; }
            public string Structure { get; set; }
            public double Margin { get; set; }
            public bool IsHighRes { get; set; }
        }

        public void InitializeStructureListView()
        {
            this.DataGrid1StructureList = new List<DataGrid1Structure>() { }; ;
            ListCollectionView collectionView = new ListCollectionView(this.DataGrid1StructureList);
            this.DataGrid1StructureCollection = collectionView;
            this.DataGrid1.ItemsSource = this.DataGrid1StructureCollection;
        }

        public void UpdateStructureListView()
        {
            this.DataGrid1StructureList.Clear();
            foreach (var structure in this.scriptcontext.StructureSet.Structures.OrderBy(u => u.Id))
            {
                if (!structure.IsEmpty && structure.HasSegment && structure.DicomType != "MARKER")
                {
                    Color newColor = structure.Color;
                    newColor.A = 50;
                    string color = newColor.ToString();
                    DataGrid1Structure item = new DataGrid1Structure()
                    {
                        Structure = structure.Id,
                        DicomType = structure.DicomType,
                        IsHighRes = structure.IsHighResolution,
                        ColorSet = color
                    };
                    this.DataGrid1StructureList.Add(item);
                }
            }
            this.DataGrid1StructureCollection.Refresh();
        }

        public void PopulateComboBoxDicomType()
        {
            this.ComboBoxDicomType.ItemsSource = new List<string>() { "", "PTV", "CTV", "GTV", "CONTROL", "ORGAN", "IRRAD_VOLUME" };
            this.ComboBoxDicomType.SelectedIndex = 0;
        }

        public void InitializeDataGrid2Structure()
        {
            this.DataGrid2StructureList = new List<DataGrid2Structure>() { };
            ListCollectionView collectionView = new ListCollectionView(this.DataGrid2StructureList);
            this.DataGrid2StructureCollection = collectionView;
            this.DataGrid2.ItemsSource = this.DataGrid2StructureCollection;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            int ind = txt.CaretIndex;
            txt.Text = txt.Text.Replace(",", ".");
            txt.CaretIndex = ind;
        }

        private void EnableCheckBoxAllowConversionHighResSegment()
        {
            foreach (var s in this.DataGrid1StructureList)
            {
                if (s.IsHighRes)
                {
                    this.CheckBoxAllowHighResConversion.IsEnabled = true;
                    return;
                }
            }
        }

        private bool CanCreateStructure()
        {
            if (this.ComboBoxDicomType.SelectedIndex != -1 && this.TextBoxStructureName.Text != "")
            {
                if (!this.scriptcontext.StructureSet.CanAddStructure(this.ComboBoxDicomType.SelectedValue.ToString(), this.TextBoxStructureName.Text))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }

        }

        private void TextBoxStructureName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!CanCreateStructure())
            {
                this.LabelCanAddStructure.Content = "Invalid structure.";
                this.LabelCanAddStructure.Foreground = Brushes.Red;
                this.TextBoxStructureName.BorderBrush = Brushes.Red;
            }
            else
            {
                this.LabelCanAddStructure.Content = "";
                this.LabelCanAddStructure.Foreground = Brushes.Black;
                this.TextBoxStructureName.BorderBrush = Brushes.Green;
            }
        }

        private void ComboBoxDicomType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!CanCreateStructure())
            {
                this.LabelCanAddStructure.Content = "Invalid structure.";
                this.LabelCanAddStructure.Foreground = Brushes.Red;
                this.TextBoxStructureName.BorderBrush = Brushes.Red;
            }
            else
            {
                this.LabelCanAddStructure.Content = "";
                this.LabelCanAddStructure.Foreground = Brushes.Black;
                this.TextBoxStructureName.BorderBrush = Brushes.Green;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Add structure
            if (this.DataGrid1.SelectedItem != null)
            {
                var selectedItem = this.DataGrid1.SelectedItem as DataGrid1Structure;

                DataGrid2Structure newItem = new DataGrid2Structure()
                {
                    Structure = selectedItem.Structure,
                    Operation = "\u2795",
                    OperationType = "add",
                    OperationColor = Colors.Red.ToString(),
                    Margin = 0,
                    IsHighRes = selectedItem.IsHighRes
                };
                this.DataGrid2StructureList.Add(newItem);
                this.DataGrid2StructureCollection.Refresh();
                this.DataGrid2.SelectedItem = newItem;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Subtract structure
            if (this.DataGrid1.SelectedItem != null)
            {
                var selectedItem = this.DataGrid1.SelectedItem as DataGrid1Structure;

                DataGrid2Structure newItem = new DataGrid2Structure()
                {
                    Structure = selectedItem.Structure,
                    Operation = "\u2796",
                    OperationType = "subtract",
                    OperationColor = Colors.Blue.ToString(),
                    Margin = 0,
                    IsHighRes = selectedItem.IsHighRes
                };
                this.DataGrid2StructureList.Add(newItem);
                this.DataGrid2StructureCollection.Refresh();
                this.DataGrid2.SelectedItem = newItem;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // Crossection
            if (this.DataGrid1.SelectedItem != null)
            {
                var selectedItem = this.DataGrid1.SelectedItem as DataGrid1Structure;

                DataGrid2Structure newItem = new DataGrid2Structure()
                {
                    Structure = selectedItem.Structure,
                    Operation = "\u2336",
                    OperationType = "intersect",
                    OperationColor = Colors.Orange.ToString(),
                    Margin = 0,
                    IsHighRes = selectedItem.IsHighRes
                };
                this.DataGrid2StructureList.Add(newItem);
                this.DataGrid2StructureCollection.Refresh();
                this.DataGrid2.SelectedItem = newItem;
            }
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            // XOR
            if (this.DataGrid1.SelectedItem != null)
            {
                var selectedItem = this.DataGrid1.SelectedItem as DataGrid1Structure;

                DataGrid2Structure newItem = new DataGrid2Structure()
                {
                    Structure = selectedItem.Structure,
                    Operation = "\u2295",
                    OperationType = "xor",
                    OperationColor = Colors.Green.ToString(),
                    Margin = 0,
                    IsHighRes = selectedItem.IsHighRes
                };
                this.DataGrid2StructureList.Add(newItem);
                this.DataGrid2StructureCollection.Refresh();
                this.DataGrid2.SelectedItem = newItem;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            // Remove structure
            if (this.DataGrid2.SelectedItem != null)
            {
                var selectedItem = this.DataGrid2.SelectedItem as DataGrid2Structure;
                var selectedIndex = this.DataGrid2.SelectedIndex;
                this.DataGrid2StructureList.RemoveAt(selectedIndex);
                this.DataGrid2StructureCollection.Refresh();
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            // Remove all
            this.DataGrid2StructureList.Clear();
            this.DataGrid2StructureCollection.Refresh();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)this.CheckBoxClipSkin.IsChecked)
            {
                this.TextBoxClipSkinMargin.IsEnabled = true;
            }
            else
            {
                this.TextBoxClipSkinMargin.IsEnabled = false;
            }
        }

        private double ConvertTextToDouble(string text)
        {
            if (Double.TryParse(text, out double result))
            {
                return result;
            }
            else
            {
                return Double.NaN;
            }
        }

        public string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public Structure CopyAndConvertToHighRes(Structure structureExisting)
        {
            string newStructureName = RandomString(10);
            string newStructureDicomType = structureExisting.DicomType;

            if (newStructureDicomType == "EXTERNAL")
            {
                newStructureDicomType = "CONTROL";
            }

            Structure newStructure = this.scriptcontext.StructureSet.AddStructure(newStructureDicomType, newStructureName);
            this.ListOfCreatedTempStructure.Add(newStructure);

            newStructure.SegmentVolume = structureExisting.SegmentVolume;

            if (newStructure.CanConvertToHighResolution())
            {
                newStructure.ConvertToHighResolution();
            }
            return newStructure;
        }

        private void DeleteTempStructures()
        {
            foreach (var str in this.ListOfCreatedTempStructure)
            {
                if (this.scriptcontext.StructureSet.CanRemoveStructure(str))
                {
                    this.scriptcontext.StructureSet.RemoveStructure(str);
                }
            }
        }

        private void ResetTempStructuresList()
        {
            this.ListOfCreatedTempStructure.Clear();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            // Calculate new structure
            if (!CanCreateStructure())
            {
                MessageBox.Show("Cannot create structure. Possible reasons: invalid name, structure already exists, invalid dicom type.", "Error");
                return;
            }

            if ((bool)this.CheckBoxClipSkin.IsChecked & Double.IsNaN(ConvertTextToDouble(this.TextBoxClipSkinMargin.Text)))
            {
                MessageBox.Show("Cannot clip structure under the body. Margin is invalid.", "Error");
                return;
            }

            if (this.DataGrid2StructureList.Count < 1)
            {
                MessageBox.Show("No operation is defined.", "Error");
                return;
            }

            foreach (var s in this.DataGrid2StructureList)
            {
                if (s.IsHighRes & !(bool)this.CheckBoxAllowHighResConversion.IsChecked)
                {
                    MessageBox.Show("At least one structure is of high resolution. If you wish to perform the operation, you must allow the conversion to High Resolution Segment.", "Error");
                    return;
                }
            }

            var waitWindow = new WaitingWindow();

            try
            {
                this.Cursor = Cursors.Wait;
                waitWindow.Show();

                string dicomType = this.ComboBoxDicomType.SelectedItem.ToString();
                string structureName = this.TextBoxStructureName.Text;

                Structure newStructure = this.scriptcontext.StructureSet.AddStructure(dicomType, structureName);

                if ((bool)this.CheckBoxAllowHighResConversion.IsChecked)
                {
                    newStructure.ConvertToHighResolution();
                }

                foreach (var item in this.DataGrid2StructureList)
                {
                    string structureString = item.Structure;
                    Structure structure = this.scriptcontext.StructureSet.Structures.First(u => u.Id == structureString);

                    // if at least one structure has HighRes convert all to highres before operation!
                    if ((bool)this.CheckBoxAllowHighResConversion.IsChecked && !structure.IsHighResolution)
                    {
                        structure = CopyAndConvertToHighRes(structure);
                    }

                    double margin = item.Margin;
                    string operation = item.OperationType;

                    if (operation == "add")
                    {
                        newStructure.SegmentVolume = newStructure.SegmentVolume.Or(structure.SegmentVolume.Margin(margin));
                    }
                    else if (operation == "subtract")
                    {
                        newStructure.SegmentVolume = newStructure.SegmentVolume.Sub(structure.SegmentVolume.Margin(margin));
                    }
                    else if (operation == "intersect")
                    {
                        newStructure.SegmentVolume = newStructure.SegmentVolume.And(structure.SegmentVolume.Margin(margin));
                    }
                    else if (operation == "xor")
                    {
                        newStructure.SegmentVolume = newStructure.SegmentVolume.Xor(structure.SegmentVolume.Margin(margin));
                    }
                }

                // Clip resulting structure near the edge of the body.
                // Body must be copied into a temporary structure and deleted at the end of operation.
                if ((bool)this.CheckBoxClipSkin.IsChecked)
                {
                    double marginClip = ConvertTextToDouble(this.TextBoxClipSkinMargin.Text);

                    Structure copiedBody = (Structure)null;
                    bool wasBodyCreated = false;

                    int i = 0;
                    while (!wasBodyCreated & i < 10)
                    {
                        string randomName = RandomString(10);
                        if (this.scriptcontext.StructureSet.CanAddStructure("CONTROL", randomName))
                        {
                            copiedBody = this.scriptcontext.StructureSet.AddStructure("CONTROL", RandomString(10));
                            wasBodyCreated = true;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    if (!wasBodyCreated)
                    {
                        MessageBox.Show("Cannot create temp structure for clipping.", "Sporočilo o napaki.");
                    }
                    else
                    {
                        this.ListOfCreatedTempStructure.Add(copiedBody);

                        Structure existingBody = this.scriptcontext.StructureSet.Structures.Where(b => b.DicomType == "EXTERNAL").OrderBy(b => b.Volume).Last();

                        copiedBody.SegmentVolume = existingBody.SegmentVolume.Margin(-marginClip);

                        if ((bool)this.CheckBoxAllowHighResConversion.IsChecked)
                        {
                            copiedBody.ConvertToHighResolution();
                        }

                        newStructure.SegmentVolume = newStructure.SegmentVolume.And(copiedBody.SegmentVolume);
                    }
                }
                DeleteTempStructures();
                ResetTempStructuresList();
            }
            catch (Exception h)
            {
                waitWindow.Close();
                this.Cursor = null;
                DeleteTempStructures();
                ResetTempStructuresList();
                MessageBox.Show(h.ToString(), "Error");
            }

            waitWindow.Close();
            this.Cursor = null;

            UpdateStructureListView();
            this.TextBoxStructureName.Text = "";
            this.ComboBoxDicomType.SelectedIndex = 0;
        }
    }
}
