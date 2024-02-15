{{/*
Expand the name of the chart.
*/}}
{{- define "viplevels-level.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "viplevels-level.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "viplevels-level.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "viplevels-level.labels" -}}
helm.sh/chart: {{ include "viplevels-level.chart" . }}
{{ include "viplevels-level.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "viplevels-level.selectorLabels" -}}
app.kubernetes.io/name: {{ include "viplevels-level.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app: {{ include "viplevels-level.name" . }}
tgs.kind: viplevels
tgs.viplevels.kind: level
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "viplevels-level.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "viplevels-level.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}
