variable "region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "cluster_name" {
  description = "EKS cluster name"
  type        = string
  default     = "minimal-eks"
}

variable "vpc_id" {
  description = "VPC ID for EKS"
  type        = string
}

variable "subnet_ids" {
  description = "Subnet IDs for EKS worker nodes"
  type        = list(string)
}

variable "eks_version" {
  description = "Kubernetes version for EKS"
  type        = string
  default     = "1.29"
}