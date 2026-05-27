using System;
using System.Reflection;
using Avalonia;
using Avalonia.Headless;
using AvaloniaEdit;
using AvaloniaEdit.Editing;

class ImeVerification
{
    static int Main(string[] args)
    {
        Console.WriteLine("=== Chinese IME Verification Test ===");
        Console.WriteLine();

        // Initialize Avalonia
        AppBuilder.Configure<Application>().UseHeadless(new AvaloniaHeadlessPlatformOptions()).SetupWithoutStarting();

        int passed = 0;
        int failed = 0;

        // Test 1: Create TextArea and verify IME client exists
        Console.Write("Test 1: TextArea creates IME client... ");
        try
        {
            var textArea = new TextArea();
            var imClientField = typeof(TextArea).GetField("_imClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var imClient = imClientField?.GetValue(textArea);
            if (imClient != null)
            {
                Console.WriteLine("PASS");
                passed++;
            }
            else
            {
                Console.WriteLine("FAIL - IME client is null");
                failed++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL - {ex.Message}");
            failed++;
        }

        // Test 2: Verify SupportsPreedit is true
        Console.Write("Test 2: SupportsPreedit returns true... ");
        try
        {
            var textArea = new TextArea();
            var imClientField = typeof(TextArea).GetField("_imClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var imClient = imClientField!.GetValue(textArea)!;
            var supportsPreeditProp = imClient.GetType().GetProperty("SupportsPreedit");
            var supportsPreedit = (bool)supportsPreeditProp!.GetValue(imClient)!;
            if (supportsPreedit)
            {
                Console.WriteLine("PASS");
                passed++;
            }
            else
            {
                Console.WriteLine("FAIL - SupportsPreedit is false");
                failed++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL - {ex.Message}");
            failed++;
        }

        // Test 3: Verify PreeditLayer field exists
        Console.Write("Test 3: PreeditLayer field exists... ");
        try
        {
            var textArea = new TextArea();
            var preeditLayerField = typeof(TextArea).GetField("_preeditLayer", BindingFlags.NonPublic | BindingFlags.Instance);
            var preeditLayer = preeditLayerField?.GetValue(textArea);
            if (preeditLayer != null)
            {
                Console.WriteLine("PASS");
                passed++;
            }
            else
            {
                Console.WriteLine("FAIL - PreeditLayer not found");
                failed++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL - {ex.Message}");
            failed++;
        }

        // Test 4: Verify SetPreeditText method exists
        Console.Write("Test 4: SetPreeditText methods exist... ");
        try
        {
            var textArea = new TextArea();
            var imClientField = typeof(TextArea).GetField("_imClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var imClient = imClientField!.GetValue(textArea)!;
            var method1 = imClient.GetType().GetMethod("SetPreeditText", new Type[] { typeof(string) });
            var method2 = imClient.GetType().GetMethod("SetPreeditText", new Type[] { typeof(string), typeof(int?) });
            if (method1 != null && method2 != null)
            {
                Console.WriteLine("PASS");
                passed++;
            }
            else
            {
                Console.WriteLine($"FAIL - method1={method1 != null}, method2={method2 != null}");
                failed++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL - {ex.Message}");
            failed++;
        }

        // Test 5: Verify ClearPreedit method exists
        Console.Write("Test 5: ClearPreedit method exists... ");
        try
        {
            var textArea = new TextArea();
            var imClientField = typeof(TextArea).GetField("_imClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var imClient = imClientField!.GetValue(textArea)!;
            var method = imClient.GetType().GetMethod("ClearPreedit");
            if (method != null)
            {
                Console.WriteLine("PASS");
                passed++;
            }
            else
            {
                Console.WriteLine("FAIL - ClearPreedit method not found");
                failed++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL - {ex.Message}");
            failed++;
        }

        // Test 6: Verify SupportsSurroundingText is true
        Console.Write("Test 6: SupportsSurroundingText returns true... ");
        try
        {
            var textArea = new TextArea();
            var imClientField = typeof(TextArea).GetField("_imClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var imClient = imClientField!.GetValue(textArea)!;
            var prop = imClient.GetType().GetProperty("SupportsSurroundingText");
            var value = (bool)prop!.GetValue(imClient)!;
            if (value)
            {
                Console.WriteLine("PASS");
                passed++;
            }
            else
            {
                Console.WriteLine("FAIL - SupportsSurroundingText is false");
                failed++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL - {ex.Message}");
            failed++;
        }

        // Test 7: Verify SetPreeditText can be called without error
        Console.Write("Test 7: SetPreeditText executes without error... ");
        try
        {
            var textArea = new TextArea();
            var imClientField = typeof(TextArea).GetField("_imClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var imClient = imClientField!.GetValue(textArea)!;
            var method = imClient.GetType().GetMethod("SetPreeditText", new Type[] { typeof(string) });
            method!.Invoke(imClient, new object[] { "zhong" });
            Console.WriteLine("PASS");
            passed++;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL - {ex.Message}");
            failed++;
        }

        // Test 8: Verify ClearPreedit can be called without error
        Console.Write("Test 8: ClearPreedit executes without error... ");
        try
        {
            var textArea = new TextArea();
            var imClientField = typeof(TextArea).GetField("_imClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var imClient = imClientField!.GetValue(textArea)!;
            var setMethod = imClient.GetType().GetMethod("SetPreeditText", new Type[] { typeof(string) });
            setMethod!.Invoke(imClient, new object[] { "test" });
            var clearMethod = imClient.GetType().GetMethod("ClearPreedit");
            clearMethod!.Invoke(imClient, null);
            Console.WriteLine("PASS");
            passed++;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL - {ex.Message}");
            failed++;
        }

        Console.WriteLine();
        Console.WriteLine($"Results: {passed} passed, {failed} failed out of {passed + failed} tests");

        if (failed > 0)
        {
            Console.WriteLine("\nSome tests failed!");
            return 1;
        }
        else
        {
            Console.WriteLine("\nAll tests passed! Chinese IME support is properly implemented.");
            return 0;
        }
    }
}
