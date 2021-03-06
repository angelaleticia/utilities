﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace WindowsForms
{
    public partial class DynamicForm : Form
    {
        public Dictionary<string, string> dicReturnValues
        {
            get { return _returnValues; }
        }

        List<DynamicControl> _dynamicControls;
        Dictionary<string, string> _returnValues;
        int _labelWidth = 150;
        int _controlWidth = 350;
        int _panelOffset = 20;
        int _controlOffset = 2;

        public DynamicForm(List<DynamicControl> dynamicControls)
        {
            InitializeComponent();
            Application.EnableVisualStyles();
            _dynamicControls = dynamicControls;
            _returnValues = new Dictionary<string, string>();
        }

        #region public methods
        public string GetValueByey(string key)
        {
            return _returnValues.ContainsKey(key) ? _returnValues[key] : null;
        }

        #endregion

        #region events methods
        private void DynamicForm_Load(object sender, System.EventArgs e)
        {
            CreateDynamicControls();
        }

        private void DynamicForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.Yes)
            {
                FillReturnValues();
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Read data and create controls.
        /// </summary>
        private void CreateDynamicControls()
        {
            System.Drawing.Point lastLocation = new System.Drawing.Point(panel1.Location.X, panel1.Location.Y);
            lastLocation.X += 5;
            lastLocation.Y += 5;
            int controlsTotalHeight = panel1.Height;

            foreach (DynamicControl ctr in _dynamicControls)
            {
                var lbl = new Label { Text = ctr.Label, Width = _labelWidth, AutoSize = false, Location = lastLocation };
                var winCtr = GetControl(ctr, lastLocation);

                if (winCtr != null)
                {
                    panel1.Controls.Add(lbl);
                    panel1.Controls.Add(winCtr);
                    lastLocation.Y = winCtr.Location.Y + winCtr.Height + 2;
                    controlsTotalHeight += winCtr.Height + _controlOffset;
                }
            }

            Height = controlsTotalHeight + (_panelOffset * 2) + btnOk.Height + _controlOffset;
            Width = _labelWidth + _controlWidth + (_controlOffset * 2) + (_panelOffset * 5);
        }

        /// <summary>
        /// Return control to be inserted in the form
        /// </summary>
        /// <param name="ctr"></param>
        /// <param name="lastLocation"></param>
        /// <returns></returns>
        private Control GetControl(DynamicControl ctr, System.Drawing.Point lastLocation)
        {
            Control winCtl = null;

            switch (ctr.ControlType)
            {
                case "TextBox":
                    winCtl = new TextBox
                    {
                        Tag = ctr.Key,
                        Name = ctr.Name,
                        Location = new System.Drawing.Point(lastLocation.X + _labelWidth + 2, lastLocation.Y),
                        Text = ctr.InitialValue.ToString(),
                        Width = _controlWidth
                    };

                    _returnValues.Add(ctr.Key, null);
                    return winCtl;

                case "ComboBox":
                    winCtl = new ComboBox
                    {
                        Tag = ctr.Key,
                        Name = ctr.Name,
                        Location = new System.Drawing.Point(lastLocation.X + _labelWidth + 2, lastLocation.Y),
                        Width = _controlWidth,
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };

                    _returnValues.Add(ctr.Key, null);

                    if (ctr.BindingSource != null)
                    {
                        if (ctr.BindingSource.Items != null)
                        {
                            (winCtl as ComboBox).Items.AddRange(ctr.BindingSource.Items.ToArray());
                        }
                        else if (!string.IsNullOrEmpty(ctr.BindingSource.ConnectionString) &&
                          !string.IsNullOrEmpty(ctr.BindingSource.DisplayValue) &&
                          !string.IsNullOrEmpty(ctr.BindingSource.KeyValue) &&
                          !string.IsNullOrEmpty(ctr.BindingSource.Sql))
                        {
                            ConfigComboBoxBinding(ctr.BindingSource.ConnectionString, ctr.BindingSource.Sql, ctr.BindingSource.KeyValue, ctr.BindingSource.DisplayValue, (winCtl as ComboBox));
                        }
                    }

                    return winCtl;

                case "DateTime":
                    winCtl = new DateTimePicker
                    {
                        Tag = ctr.Key,
                        Name = ctr.Name,
                        Location = new System.Drawing.Point(lastLocation.X + _labelWidth + 2, lastLocation.Y),
                        Format = DateTimePickerFormat.Short,
                        Value = ctr.InitialValue != null ? DateTime.Parse(ctr.InitialValue.ToString()) : DateTime.Now,
                        Width = _controlWidth
                    };

                    _returnValues.Add(ctr.Key, null);

                    return winCtl;

                case "CheckBox":
                    winCtl = new CheckBox
                    {
                        Tag = ctr.Key,
                        Name = ctr.Name,
                        Location = new System.Drawing.Point(lastLocation.X + _labelWidth + 2, lastLocation.Y),
                        Checked = ctr.InitialValue != null ? Convert.ToBoolean(ctr.InitialValue) : false,
                        Width = _controlWidth
                    };

                    _returnValues.Add(ctr.Key, null);

                    return winCtl;
            }

            return winCtl;
        }

        /// <summary>
        /// Configur combo box data binding
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sql"></param>
        /// <param name="keyValue"></param>
        /// <param name="displayValue"></param>
        /// <param name="cb"></param>
        private void ConfigComboBoxBinding(string connectionString, string sql, string keyValue, string displayValue, ComboBox cb)
        {
            using (DataTable dt = DataConnection.GetDataTable(connectionString, sql))
            {
                if (dt == null)
                {
                    return;
                }

                cb.DisplayMember = displayValue;
                cb.ValueMember = keyValue;
                cb.DataSource = dt.DefaultView;
            }
        }

        /// <summary>
        /// Send control values and text to return dictionary.
        /// </summary>
        private void FillReturnValues()
        {
            foreach (Control ctl in panel1.Controls)
            {
                if (ctl.Tag == null || !_returnValues.ContainsKey(ctl.Tag.ToString()))
                {
                    continue;
                }

                if (ctl is ComboBox)
                {
                    _returnValues[ctl.Tag.ToString()] = (ctl as ComboBox).SelectedValue.ToString();
                }

                if (ctl is TextBox)
                {
                    _returnValues[ctl.Tag.ToString()] = (ctl as TextBox).Text;
                }

                if (ctl is CheckBox)
                {
                    _returnValues[ctl.Tag.ToString()] = (ctl as CheckBox).Checked.ToString();
                }

                if (ctl is DateTimePicker)
                {
                    _returnValues[ctl.Tag.ToString()] = (ctl as DateTimePicker).Value.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
        } 
        #endregion
    }
}
