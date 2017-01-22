package main

import "fmt"

type Token struct {
	data         string
	sender       int
	recipent     int
	killerChanel chan string
}

func frodo(in chan Token, out chan Token, threadNum int) {
	ring := <-in
	if ring.sender == threadNum {
		fmt.Println("Thread", threadNum, ": recipent not found")
		ring.killerChanel <- "exit"
	} else {
		switch ring.recipent {
		case threadNum:
			fmt.Println("Thread", threadNum, ": message recived")
			fmt.Println("Thread", threadNum, ": Message:", ring.data)
			ring.killerChanel <- "exit"
		default:
			fmt.Println("Thread", threadNum, ": forward message")
			out <- ring
		}
	}
}

func main() {
	const numThreads = 10
	const sender = 1
	const reciver = 9

	var killerChanel chan string = make(chan string)
	ring := Token{"There can be your advertisment. Call 8-999-888-4444.", sender, reciver, killerChanel}

	var channels [numThreads]chan Token
	for i := 0; i < numThreads; i++ {
		channels[i] = make(chan Token)
	}

	for i := 0; i < numThreads-1; i++ {
		go frodo(channels[i], channels[i+1], i+1)
	}

	// make loop
	go frodo(channels[numThreads-1], channels[0], numThreads)

	fmt.Println("Thread", sender, ": send message")
	channels[sender] <- ring

	<-killerChanel
}
