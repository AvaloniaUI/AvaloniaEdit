#!/usr/bin/julia

function BubbleSort(arr)
    l = length(arr)
    swapped = true
    while swapped
        swapped = false
        for j = 2:l
            if arr[j-1] > arr[j]
                tmp = arr[j-1]
                arr[j-1] = arr[j]
                arr[j] = tmp
                swapped = true
            end
        end
    end
end

function error()
    println("Usage: please provide a list of at least two integers to sort in the format \"1, 2, 3, 4, 5\"")
end

function HandleInput(inp)

    a = split(inp,",")
    a = map(x->split(x," "),a)
    a = map(x->last(x),a)
    numbers = map(x->parse(Int,x),a)
    if length(numbers) == 1
        error()
        exit()
    end
    return numbers
    
end

function PrintOutput(out)
    for i = 1:length(out)
        print(out[i])
        if i != length(out)
            print(", ")
        end
    end
    println()
end

try

    n = HandleInput(ARGS[1])
    BubbleSort(n)
    PrintOutput(n)

catch e
    error()
end


