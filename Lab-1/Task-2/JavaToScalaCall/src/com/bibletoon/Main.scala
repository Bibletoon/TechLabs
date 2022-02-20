package com.bibletoon

import scala.util.chaining.scalaUtilChainingOps;

sealed trait Identifier;
case class Username(firstName : String, secondName: String) extends Identifier;
case class Email(value : String) extends Identifier;

object Scala {
  def checkUser(id: Identifier) = id match {
    case Username(firstName, secondName) => Left(secondName+" "+firstName)
    case Email(email) => Right(email.reverse)
  }

  def main(args: Array[String]) = {
    checkUser(Username("aboba", "abobievich")) match {
      case Left(value) => value.pipe(println).pipe(println)
      case Right(value) => println(value)
    }
  }
}