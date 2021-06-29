fizz_buzz <- function(){
  for (x in 1:100){
    out<-''
    mod3<- x%%3==0
    mod5 <- x%%5==0
    if (!mod3 && !mod5){
      out=x
    }
    if (mod3){
      out='Fizz'
    }
    if (mod5){
      out=paste0(out,'Buzz')
    }
    cat(out, "\n")
  }
}

fizz_buzz()
