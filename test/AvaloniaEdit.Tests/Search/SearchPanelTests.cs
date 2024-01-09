using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using AvaloniaEdit.AvaloniaMocks;
using NUnit.Framework;

namespace AvaloniaEdit.Search;

[TestFixture]
public class SearchPanelTests
{
    [AvaloniaTest]
    public void Find_Next_Should_Find_Next_Occurence()
    {
        UnitTestApplication.InitializeStyles();

        TextEditor textEditor = CreateEditor();
        textEditor.Text = "hello world";

        textEditor.SearchPanel.SearchPattern = "world";
        textEditor.SearchPanel.Open();
        textEditor.SearchPanel.FindNext();

        Assert.AreEqual(6, textEditor.SelectionStart);
        Assert.AreEqual(5, textEditor.SelectionLength);
    }

    [AvaloniaTest]
    public void Find_Next_After_Find_Next_Should_Find_The_Occurence()
    {
        UnitTestApplication.InitializeStyles();

        TextEditor textEditor = CreateEditor();
        textEditor.Text = "hello world world";

        textEditor.SearchPanel.SearchPattern = "world";
        textEditor.SearchPanel.Open();
        textEditor.SearchPanel.FindNext();
        textEditor.SearchPanel.FindNext();

        Assert.AreEqual(12, textEditor.SelectionStart);
        Assert.AreEqual(5, textEditor.SelectionLength);
    }

    [AvaloniaTest]
    public void Find_Previous_Should_Find_Previous_Occurence()
    {
        UnitTestApplication.InitializeStyles();

        TextEditor textEditor = CreateEditor();
        textEditor.Text = "hello world";

        textEditor.CaretOffset = 6;

        textEditor.SearchPanel.SearchPattern = "hello";
        textEditor.SearchPanel.Open();
        textEditor.SearchPanel.FindPrevious();

        Assert.AreEqual(0, textEditor.SelectionStart);
        Assert.AreEqual(5, textEditor.SelectionLength);
    }

    [AvaloniaTest]
    public void Find_Previous_After_Find_Previous_Should_Find_Occurence()
    {
        UnitTestApplication.InitializeStyles();

        TextEditor textEditor = CreateEditor();
        textEditor.Text = "hello world world";

        textEditor.CaretOffset = 17;

        textEditor.SearchPanel.SearchPattern = "world";
        textEditor.SearchPanel.Open();
        textEditor.SearchPanel.FindPrevious();
        textEditor.SearchPanel.FindPrevious();

        Assert.AreEqual(6, textEditor.SelectionStart);
        Assert.AreEqual(5, textEditor.SelectionLength);
    }

    [AvaloniaTest]
    public void Replace_Next_Should_Replace_Occurence()
    {
        UnitTestApplication.InitializeStyles();

        TextEditor textEditor = CreateEditor();
        textEditor.Text = "hello world";

        textEditor.CaretOffset = 6;

        textEditor.SearchPanel.SearchPattern = "world";
        textEditor.SearchPanel.ReplacePattern = "universe";
        textEditor.SearchPanel.Open();
        textEditor.SearchPanel.IsReplaceMode = true;
        textEditor.SearchPanel.ReplaceNext();

        Assert.AreEqual("hello universe", textEditor.Text);
    }

    [AvaloniaTest]
    public void Replace_Next_Should_Replace_Occurence_At_Caret_Index()
    {
        UnitTestApplication.InitializeStyles();

        TextEditor textEditor = CreateEditor();
        textEditor.Text = "hello world world";

        textEditor.CaretOffset = 6;

        textEditor.SearchPanel.SearchPattern = "world";
        textEditor.SearchPanel.ReplacePattern = "universe";
        textEditor.SearchPanel.Open();
        textEditor.SearchPanel.IsReplaceMode = true;
        textEditor.SearchPanel.ReplaceNext();

        Assert.AreEqual("hello universe world", textEditor.Text);
    }

    [AvaloniaTest]
    public void Replace_All_Should_Replace_All_Occurences()
    {
        UnitTestApplication.InitializeStyles();

        TextEditor textEditor = CreateEditor();
        textEditor.Text = "hello world hello world";

        textEditor.SearchPanel.SearchPattern = "hello";
        textEditor.SearchPanel.ReplacePattern = "bye";
        textEditor.SearchPanel.Open();
        textEditor.SearchPanel.IsReplaceMode = true;
        textEditor.SearchPanel.ReplaceAll();

        Assert.AreEqual("bye world bye world", textEditor.Text);
    }

    static TextEditor CreateEditor()
    {
        TextEditor textEditor = new TextEditor();

        Window window = new Window();
        window.Content = textEditor;

        window.Show();
        textEditor.ApplyTemplate();

        return textEditor;
    }
}