﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using pwiz.Common.DataBinding;
using pwiz.Skyline.Controls;
using pwiz.Skyline.Model;
using pwiz.Skyline.Model.Databinding;
using pwiz.Skyline.Model.Databinding.Entities;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Model.DocSettings.MetadataExtraction;
using pwiz.Skyline.Model.ElementLocators.ExportAnnotations;
using pwiz.Skyline.Properties;
using pwiz.Skyline.Util;

namespace pwiz.Skyline.SettingsUI
{
    public partial class MetadataRuleEditor : FormEx
    {
        private SkylineDataSchema _dataSchema;

        public MetadataRuleEditor(IDocumentContainer documentContainer)
        {
            InitializeComponent();
            _dataSchema = new SkylineDataSchema(documentContainer, SkylineDataSchema.GetLocalizedSchemaLocalizer());
            var rootColumn = ColumnDescriptor.RootColumn(_dataSchema, typeof(ResultFile));
            var viewContext =
                new SkylineViewContext(rootColumn, new StaticRowSource(new ExtractedMetadataResultRow[0]));
            bindingListSource1.SetViewContext(viewContext);
            var allColumns = GetAllTextColumnWrappers(rootColumn).ToList();
            var sources = allColumns.Where(IsSource).ToArray();
            comboSourceText.Items.AddRange(sources);
            comboMetadataTarget.Items.AddRange(allColumns.Where(IsTarget).ToArray());
            SelectItem(comboSourceText, PropertyPath.Root.Property(nameof(ResultFile.FileName)));
            FormatCultureInfo = CultureInfo.InvariantCulture;
        }

        public MetadataRule MetadataRule
        {
            get
            {
                var rule = new MetadataRule();
                var source = comboSourceText.SelectedItem as TextColumnWrapper;
                if (source != null)
                {
                    rule = rule.ChangeSource(source.PropertyPath);
                }

                if (!string.IsNullOrEmpty(tbxRegularExpression.Text))
                {
                    rule = rule.ChangePattern(tbxRegularExpression.Text);
                }

                if (!string.IsNullOrEmpty(tbxReplacement.Text))
                {
                    rule = rule.ChangeReplacement(tbxReplacement.Text);
                }

                var target = comboMetadataTarget.SelectedItem as TextColumnWrapper;
                if (target != null)
                {
                    rule = rule.ChangeTarget(target.PropertyPath);
                }

                return rule;
            }
            set
            {
                SelectItem(comboSourceText, value.Source);
                tbxRegularExpression.Text = value.Pattern;
                tbxReplacement.Text = value.Replacement;
                SelectItem(comboMetadataTarget, value.Target);
            }
        }

        private void SelectItem(ComboBox combo, PropertyPath propertyPath)
        {
            if (propertyPath != null && !propertyPath.IsRoot)
            {
                for (int i = 0; i < combo.Items.Count; i++)
                {
                    var item = (TextColumnWrapper)combo.Items[i];
                    if (Equals(item.PropertyPath, propertyPath))
                    {
                        combo.SelectedIndex = i;
                        return;
                    }
                }
            }
            combo.SelectedIndex = -1;
        }

        public CultureInfo FormatCultureInfo { get; set; }

        public void UpdateRows()
        {
            var metadataExtractor = new MetadataExtractor(_dataSchema, typeof(ResultFile));
            var resolvedRule = metadataExtractor.ResolveRule(MetadataRule);
            var rows = new List<ExtractedMetadataResultRow>();
            foreach (var resultFile in _dataSchema.ResultFileList.Values)
            {
                var row = new ExtractedMetadataResultRow(resultFile);
                var result = metadataExtractor.ApplyRule(resultFile, resolvedRule);
                row.AddRuleResult(null, result);
                rows.Add(row);
            }

            var viewInfo = GetDefaultViewInfo(resolvedRule, rows);
            bindingListSource1.SetView(viewInfo, new StaticRowSource(rows));
        }

        public ViewInfo GetDefaultViewInfo(ResolvedMetadataRule resolvedMetdataRule, ICollection<ExtractedMetadataResultRow> rows)
        {
            var columns = new List<ColumnSpec>();
            var ruleResults = PropertyPath.Root.Property(nameof(ExtractedMetadataResultRow.RuleResults))
                .LookupAllItems();
            columns.Add(new ColumnSpec(PropertyPath.Root.Property(nameof(ExtractedMetadataResultRow.SourceObject))).SetCaption(ColumnCaptions.ResultFile));
            if (resolvedMetdataRule.Source != null)
            {
                columns.Add(new ColumnSpec(ruleResults.Property(nameof(ExtractedMetadataRuleResult.Source))).SetCaption(resolvedMetdataRule.Source.DisplayName));
            }

            if (rows.Any(row => !row.RuleResults.FirstOrDefault()?.Match == true))
            {
                columns.Add(new ColumnSpec(ruleResults.Property(nameof(ExtractedMetadataRuleResult.Match))));
            }

            if (rows.Any(ShowMatchedValueColumn))
            {
                columns.Add(new ColumnSpec(ruleResults.Property(nameof(ExtractedMetadataRuleResult.MatchedValue))));
            }

            if (rows.Any(ShowReplacedValueColumn))
            {
                columns.Add(new ColumnSpec(ruleResults.Property(nameof(ExtractedMetadataRuleResult.ReplacedValue))));
            }

            if (resolvedMetdataRule.Target != null)
            {
                columns.Add(new ColumnSpec(ruleResults.Property(nameof(ExtractedMetadataRuleResult.TargetValue))).SetCaption(resolvedMetdataRule.Target.DisplayName));
            }

            var viewSpec = new ViewSpec().SetColumns(columns).SetSublistId(ruleResults);
            var rootColumn = ColumnDescriptor.RootColumn(_dataSchema, typeof(ExtractedMetadataResultRow));
            return new ViewInfo(rootColumn, viewSpec);
        }

        public bool ShowMatchedValueColumn(ExtractedMetadataResultRow row)
        {
            return row.RuleResults.Any(result =>
            {
                if (!result.Match)
                {
                    return false;
                }

                if (result.MatchedValue == result.Source)
                {
                    return false;
                }
                if (result.ReplacedValue == result.MatchedValue && result.MatchedValue == Convert.ToString(result.TargetValue))
                {
                    return false;
                }

                return true;
            });
        }

        public bool ShowReplacedValueColumn(ExtractedMetadataResultRow row)
        {
            if (!ShowMatchedValueColumn(row))
            {
                return false;
            }

            return row.RuleResults.Any(result =>
            {
                if (!string.IsNullOrEmpty(result.ErrorText))
                {
                    return true;
                }

                if (result.MatchedValue == result.ReplacedValue)
                {
                    return false;
                }

                if (result.ReplacedValue == Convert.ToString(result.TargetValue))
                {
                    return false;
                }

                return true;
            });
        }

        public void ShowRegexError(Exception e)
        {
            if (e == null)
            {
                tbxRegularExpression.BackColor = SystemColors.Window;
            }
            else
            {
                tbxRegularExpression.BackColor = Color.Red;
            }
        }

        private void tbxRegularExpression_Leave(object sender, EventArgs e)
        {
            UpdateRows();
        }

        private void comboSourceText_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRows();
        }

        private void linkLabelRegularExpression_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebHelpers.OpenRegexDocLink(this);
        }

        public IEnumerable<TextColumnWrapper> GetAllTextColumnWrappers(ColumnDescriptor columnDescriptor)
        {
            var columns = Enumerable.Empty<TextColumnWrapper>();
            if (columnDescriptor.CollectionInfo != null || columnDescriptor.IsAdvanced)
            {
                return columns;
            }

            if (!typeof(SkylineObject).IsAssignableFrom(columnDescriptor.PropertyType)
                && !@"Locator".Equals(columnDescriptor.PropertyPath.Name))
            {
                columns = columns.Append(new TextColumnWrapper(columnDescriptor));
            }

            columns = columns.Concat(columnDescriptor.GetChildColumns().SelectMany(GetAllTextColumnWrappers));

            return columns;
        }

        public bool IsSource(TextColumnWrapper column)
        {
            return !IsTarget(column);
        }

        public bool IsTarget(TextColumnWrapper column)
        {
            if (column.IsImportable)
            {
                return true;
            }

            if (column.PropertyPath.Name.StartsWith(AnnotationDef.ANNOTATION_PREFIX))
            {
                return true;
            }

            return false;
        }

        private void comboMetadataTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRows();
        }

        public void OkDialog()
        {
            var helper = new MessageBoxHelper(this);
            if (!(comboSourceText.SelectedItem is TextColumnWrapper))
            {
                helper.ShowTextBoxError(comboSourceText, Resources.MessageBoxHelper_ValidateNameTextBox__0__cannot_be_empty);
                return;
            }

            if (!string.IsNullOrEmpty(tbxRegularExpression.Text))
            {
                try
                {
                    var _ = new Regex(tbxRegularExpression.Text);
                }
                catch (Exception)
                {
                    helper.ShowTextBoxError(tbxRegularExpression,
                        "{0} must either be a valid regular expression or blank");
                    return;
                }
            }

            if (!(comboMetadataTarget.SelectedItem is TextColumnWrapper))
            {
                helper.ShowTextBoxError(comboMetadataTarget, Resources.MessageBoxHelper_ValidateNameTextBox__0__cannot_be_empty);
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            OkDialog();
        }
    }
}
