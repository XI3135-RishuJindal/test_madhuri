terraform {
  required_version = ">= 1.5.7"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = ">= 6.40.0"
    }
  }
  backend "s3" {
    bucket         = "eks-tfstate"
    key            = "env/eks-cluster.tfstate"
    region         = "us-east-1"
    dynamodb_table = "eks-tfstate-lock"
  }
}

provider "aws" {
  region = var.region
}