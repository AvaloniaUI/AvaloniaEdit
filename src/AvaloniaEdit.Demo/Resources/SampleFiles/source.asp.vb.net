Public Module BubbleSort
    Public Sub Main(args As String())
        Console.WriteLine("GetCommandLineArgs: {0}", String.Join(", ", args))
        Dim listOfStringInputs = args.ToList
        Dim sortArray = listOfStringInputs.ConvertAll(Function(inputs) Int32.Parse(inputs)).ToArray

        sortArray = BubbleSort(sortArray)
        Dim outputSorted As String = String.Join(", ", sortArray)
        System.Console.WriteLine($"Sorted: {outputSorted}")
    End Sub

    Public Function BubbleSort(sortArray As Integer())
        Dim holdInt
        For i = 0 To UBound(sortArray)
            For x = UBound(sortArray) To i + 1 Step -1
                If sortArray(x) < sortArray(i) Then
                    holdInt = sortArray(x)
                    sortArray(x) = sortArray(i)
                    sortArray(i) = holdInt
                End If
            Next x
        Next i
        Return sortArray
    End Function
End Module
