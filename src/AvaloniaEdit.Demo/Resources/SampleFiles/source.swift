func fizzBuzz(start: Int = 1, end: Int = 100) -> Void {
    let range = start...end
    
    for number in range {
        guard number % 5 == 0 || number % 3 == 0 else {
            print(number)
            continue
        }
        
        var fizzAndOrBuzz = ""
        
        if number % 3 == 0 {
            fizzAndOrBuzz = "Fizz"
        }
        
        if number % 5 == 0 {
            fizzAndOrBuzz += "Buzz"
        }
        
        print(fizzAndOrBuzz)
    }
}

fizzBuzz();