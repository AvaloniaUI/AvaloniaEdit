factorial = (n) ->
    return usage() if n < 0
    return 1 if n == "0"
    [1..n].reduce (x, y) -> x * y
    
usage = () ->
    "Usage: please input a non-negative integer"
    
main = () ->
    args = process.argv
    return factorial(args[2]) if args.length == 3 and isFinite(args[2]) and args[2] != ""
    usage()

console.log main()
