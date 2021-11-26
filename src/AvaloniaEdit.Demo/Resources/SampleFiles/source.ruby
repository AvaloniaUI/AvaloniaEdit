def bubble_sort(numbers)
  n = numbers.length
  for i in 0...n-1
    for j in 0...n-i-1
      if numbers[j] > numbers[j + 1]
        numbers[j], numbers[j + 1] = numbers[j + 1], numbers[j]
      end
    end
  end
  return numbers
end

def err()
  puts('Usage: please provide a list of at least two integers to sort in the format "1, 2, 3, 4, 5"')
end

begin
  unsorted = ARGV[0].split(", ").map{|i| Integer(i)}
  if unsorted.length > 1
    sorted = bubble_sort(unsorted)
    print(sorted)
  else
    err()
  end
rescue
  err()
end
